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
}

func ParseTQL(input string) (*Table, error) {
	open := strings.Index(input, "(")
	close := strings.LastIndex(input, ")")
	if open == -1 || close == -1 || close <= open {
		return nil, fmt.Errorf("invalid TQL syntax")
	}

	name := strings.TrimSpace(input[:open])
	cols := strings.Split(input[open+1:close], ",")

	var fields []Column
	for _, raw := range cols {
		c := strings.TrimSpace(raw)
		if c == "" {
			continue
		}

		col := Column{
			Type:     "TEXT",
			Nullable: false,
			Unique:   false,
			IsPK:     false,
			IsFK:     false,
		}

		nameEnd := strings.IndexAny(c, "?!+-*")
		if nameEnd == -1 {
			col.Name = c
		} else {
			col.Name = c[:nameEnd]
			suffix := c[nameEnd:]
			for _, r := range suffix {
				switch r {
				case '?':
					col.Nullable = true
				case '!':
					col.Unique = true
				case '+':
					col.Type = "INTEGER"
				case '-':
					col.Type = "TEXT"
				case '*':
					col.IsPK = true
				}
			}
		}

		if col.Name == "Id" && !col.IsPK {
			col.Type = "INTEGER"
			col.IsPK = true
		}

		if strings.HasSuffix(col.Name, "Id") && col.Name != "Id" {
			col.IsFK = true
			col.FKTable = strings.TrimSuffix(col.Name, "Id")
		}

		fields = append(fields, col)
	}

	t := &Table{Name: name, Fields: fields}
	return t, nil
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
		if col.IsPK {
			line += " PRIMARY KEY"
		}
		if i < len(t.Fields)-1 {
			line += ","
		}
		lines = append(lines, line)
	}

	for _, col := range t.Fields {
		if col.IsFK {
			lines = append(lines, fmt.Sprintf("  ,FOREIGN KEY (%s) REFERENCES %s(Id)", col.Name, col.FKTable))
		}
	}

	lines = append(lines, ");")
	return strings.Join(lines, "\n")
}
