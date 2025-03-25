// cmd/sqraft/main.go
package main

import (
	"fmt"
	"os"

	"github.com/Aha43/sqraft/internal/tql"
)

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
}
