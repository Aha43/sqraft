// internal/tql/parser.go
package tql

import (
	"fmt"
	"path/filepath"
	"strings"
	"io/ioutil"
)

type Column struct {
	Name     string
	Type     string // "INTEGER" or "TEXT"
	Nullable bool
	Unique   bool
	IsPK     bool
	IsFK     bool
	FKTable  string
	FKColumn string
}

type Table struct {
	Name        string
	Fields      []Column
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

		aPK, aType := loadPKColumnAndType(a, "tql")
		bPK, bType := loadPKColumnAndType(b, "tql")

		// aPK, err := LoadPKColumn(a, "tql")
		// if err != nil {
		// 	aPK = "Id"
		// }
		// bPK, err := LoadPKColumn(b, "tql")
		// if err != nil {
		// 	bPK = "Id"
		// }

		aCol := Column{Name: a + aPK, Type: aType, Nullable: false, IsFK: true, FKTable: a, FKColumn: aPK}
		bCol := Column{Name: b + bPK, Type: bType, Nullable: false, IsFK: true, FKTable: b, FKColumn: bPK}

		fields = append(fields, aCol, bCol)

		if useCompositePK {
			compositePK = []string{aCol.Name, bCol.Name}
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
			pkCol, err := LoadPKColumn(col.FKTable, "tql")
			if err != nil {
				pkCol = "Id"
			}
			col.FKColumn = pkCol
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
			lines = append(lines, fmt.Sprintf("  ,FOREIGN KEY (%s) REFERENCES %s(%s)", col.Name, col.FKTable, col.FKColumn))
		}
	}

	lines = append(lines, ");")
	return strings.Join(lines, "\n")
}

// LoadPKColumn parses a .tql file and returns the name of the primary key column
func LoadPKColumn(tableName, tqlDir string) (string, error) {
	path := filepath.Join(tqlDir, tableName + ".tql")
	data, err := ioutil.ReadFile(path)
	if err != nil {
		return "", fmt.Errorf("failed to read TQL file for %s: %w", tableName, err)
	}

	table, err := ParseTQL(strings.TrimSpace(string(data)))
	if err != nil {
		return "", fmt.Errorf("failed to parse TQL for %s: %w", tableName, err)
	}

	for _, col := range table.Fields {
		if col.IsPK {
			return col.Name, nil
		}
	}

	return "", fmt.Errorf("no primary key found for table %s", tableName)
}

// loadPKColumnAndType returns both name and type of the primary key column
func loadPKColumnAndType(tableName, tqlDir string) (string, string) {
	path := filepath.Join(tqlDir, tableName + ".tql")
	data, err := ioutil.ReadFile(path)
	if err != nil {
		return "Id", "INTEGER"
	}

	table, err := ParseTQL(strings.TrimSpace(string(data)))
	if err != nil {
		return "Id", "INTEGER"
	}

	for _, col := range table.Fields {
		if col.IsPK {
			return col.Name, col.Type
		}
	}

	return "Id", "INTEGER"
}
