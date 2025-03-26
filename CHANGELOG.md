# Changelog

All notable changes to **Sqraft** will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] - 2025-03-26

### Added
- Initial TQL language definition
- CLI parser for parsing and generating SQL `CREATE TABLE` statements
- Automatic output of SQL files to `./sql/NNNN_TableName.sql`
- Autonumbering with detection of highest existing script number
- Overwrite prompts when table file already exists
- Support for primary key (`*`), uniqueness (`!`), nullable (`?`), integer (`+`), and text (`-`) suffixes
- Special handling for `Id` as implicit primary key
- Foreign key recognition for `SomethingId` patterns

### Notes
- This version is the foundation for future TQL tooling
- TQL syntax is stable and usable in real-world prototyping workflows
