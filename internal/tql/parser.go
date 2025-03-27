// internal/tql/parser.go
package tql

import (
	"fmt"
	"strings"
)

type Column struct {
	Name     string
	Type     string // "INTEGER" or "TEXT"
	Nullable bool
	Unique   bool
	IsPK     bool
	IsFK     bool
	FKTable  string
}

type Table struct {
	Name   string
	Fields []Column
	CompositePK []string // used only when composite PK is required
}

func ParseTQL(input string) (*Table, error) {
	open := strings.Index(input, "(")
	close := strings.LastIndex(input, ")")
	if open == -1 {
		open = len(input)
		close = len(input)
	}

	tableNameRaw := strings.TrimSpace(input[:open])
	columns := []string{}
	if open < close {
		columns = strings.Split(input[open+1:close], ",")
	}

	isJoin := false
	useCompositePK := false
	joinTables := []string{}

	if strings.Contains(tableNameRaw, "+") {
		isJoin = true
		useCompositePK = true
		joinTables = strings.Split(tableNameRaw, "+")
	} else if strings.Contains(tableNameRaw, "-") {
		isJoin = true
		joinTables = strings.Split(tableNameRaw, "-")
	}

	tableName := tableNameRaw
	if isJoin && len(joinTables) == 2 {
		tableName = joinTables[0] + "_" + joinTables[1]
	}

	var fields []Column
	var compositePK []string
	userDefinedPK := false

	if isJoin && len(joinTables) == 2 {
		a, b := joinTables[0], joinTables[1]
		aId := a + "Id"
		bId := b + "Id"

		fields = append(fields,
			Column{Name: aId, Type: "INTEGER", Nullable: false, IsFK: true, FKTable: a},
			Column{Name: bId, Type: "INTEGER", Nullable: false, IsFK: true, FKTable: b},
		)

		if useCompositePK {
			compositePK = []string{aId, bId}
		} else {
			fields = append([]Column{{Name: "Id", Type: "INTEGER", Nullable: false, IsPK: true}}, fields...)
		}
	} else {
		// we will determine later if Id should be added
	}

	for _, raw := range columns {
		c := strings.TrimSpace(raw)
		if c == "" {
			continue
		}

		col := Column{Type: "TEXT"}
		nameEnd := strings.IndexAny(c, "?!+-*")
		if nameEnd == -1 {
			col.Name = c
		} else {
			col.Name = c[:nameEnd]
			suffix := c[nameEnd:]
			for _, r := range suffix {
				switch r {
				case '?': col.Nullable = true
				case '!': col.Unique = true
				case '+': col.Type = "INTEGER"
				case '-': col.Type = "TEXT"
				case '*':
					col.IsPK = true
					userDefinedPK = true
				}
			}
		}

		if col.Name == "Id" && !col.IsPK {
			col.Type = "INTEGER"
			col.IsPK = true
			userDefinedPK = true
		}

		if strings.HasSuffix(col.Name, "Id") && col.Name != "Id" {
			col.IsFK = true
			col.FKTable = strings.TrimSuffix(col.Name, "Id")
		}

		fields = append(fields, col)
	}

	if !isJoin && !userDefinedPK {
		fields = append([]Column{{Name: "Id", Type: "INTEGER", Nullable: false, IsPK: true}}, fields...)
	}

	return &Table{Name: tableName, Fields: fields, CompositePK: compositePK}, nil
}

func (t *Table) ToSQL() string {
	lines := []string{fmt.Sprintf("CREATE TABLE %s (", t.Name)}

	for i, col := range t.Fields {
		line := "  " + col.Name + " " + col.Type
		if !col.Nullable {
			line += " NOT NULL"
		}
		if col.Unique {
			line += " UNIQUE"
		}
		if col.IsPK && len(t.CompositePK) == 0 {
			line += " PRIMARY KEY"
		}
		if i < len(t.Fields)-1 || len(t.CompositePK) > 0 {
			line += ","
		}
		lines = append(lines, line)
	}

	if len(t.CompositePK) > 0 {
		lines = append(lines, fmt.Sprintf("  PRIMARY KEY (%s)", strings.Join(t.CompositePK, ", ")))
	}

	for _, col := range t.Fields {
		if col.IsFK {
			lines = append(lines, fmt.Sprintf("  ,FOREIGN KEY (%s) REFERENCES %s(Id)", col.Name, col.FKTable))
		}
	}

	lines = append(lines, ");")
	return strings.Join(lines, "\n")
}
