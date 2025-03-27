// cmd/sqraft/main.go
package main

import (
	"bufio"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"

	"github.com/Aha43/sqraft/internal/tql"
)

var filePattern = regexp.MustCompile(`^(\d+)_([A-Za-z0-9_]+)\.sql$`)

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: sqraft \"TableName(Id, Name, Email!)\"")
		os.Exit(1)
	}

	input := os.Args[1]
	table, err := tql.ParseTQL(input)
	if err != nil {
		fmt.Println("Error:", err)
		os.Exit(1)
	}

	sql := table.ToSQL()
	fmt.Println(sql)

	sqlDir := "sql"
	tqlDir := "tql"
	for _, dir := range []string{sqlDir, tqlDir} {
		err = os.MkdirAll(dir, 0755)
		if err != nil {
			fmt.Printf("Error creating %s directory: %v\n", dir, err)
			os.Exit(1)
		}
	}

	existingFiles, err := filepath.Glob(filepath.Join(sqlDir, "*.sql"))
	if err != nil {
		fmt.Println("Error scanning sql directory:", err)
		os.Exit(1)
	}

	tableName := table.Name
	highestPrefix := 0
	matchingFile := ""

	for _, file := range existingFiles {
		base := filepath.Base(file)
		matches := filePattern.FindStringSubmatch(base)
		if len(matches) != 3 {
			continue
		}
		prefixStr, name := matches[1], matches[2]
		prefix, _ := strconv.Atoi(prefixStr)
		if prefix > highestPrefix {
			highestPrefix = prefix
		}
		if strings.EqualFold(name, tableName) {
			matchingFile = file
		}
	}

	var sqlFilename string
	if matchingFile != "" {
		fmt.Printf("File for table %s already exists: %s\nOverwrite? [y/N]: ", tableName, filepath.Base(matchingFile))
		scanner := bufio.NewScanner(os.Stdin)
		if scanner.Scan() {
			response := strings.TrimSpace(scanner.Text())
			if strings.ToLower(response) != "y" {
				fmt.Println("Aborted.")
				return
			}
		}
		sqlFilename = matchingFile
	} else {
		sqlFilename = filepath.Join(sqlDir, fmt.Sprintf("%04d_%s.sql", highestPrefix+1, tableName))
	}

	err = os.WriteFile(sqlFilename, []byte(sql), 0644)
	if err != nil {
		fmt.Println("Error writing SQL file:", err)
		os.Exit(1)
	}
	fmt.Println("Written to", sqlFilename)

	// Always write TQL file without prompting
	tqlFilename := filepath.Join(tqlDir, tableName+".tql")
	err = os.WriteFile(tqlFilename, []byte(input+"\n"), 0644)
	if err != nil {
		fmt.Println("Error writing TQL file:", err)
		os.Exit(1)
	}
	fmt.Println("Written to", tqlFilename)
}
