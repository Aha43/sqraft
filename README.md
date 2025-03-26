# sqraft

Tools for faster prototyping sql schemas.
---

## 🧠 TQL: Table Quick Language

TQL defines a single SQL table in a compact one-liner format:

```tql
TableName(Column1, Column2Suffixes, Column3Suffixes...)
```

### 🧬 Column Modifiers (Suffixes)

| Symbol | Meaning                        | SQL Effect                  |
|--------|--------------------------------|-----------------------------|
| `*`    | Primary key                    | `PRIMARY KEY`              |
| `!`    | Unique                         | `UNIQUE`                   |
| `?`    | Nullable                       | omit `NOT NULL`            |
| `+`    | Integer type                   | `INTEGER`                  |
| `-`    | Text type                      | `TEXT` (default anyway)    |

### Special rules

- A column named `Id` becomes `INTEGER PRIMARY KEY` by default
- Any column ending in `Id` (e.g. `CategoryId`) becomes a foreign key referencing that table (`Category`)
- Columns are `TEXT NOT NULL` by default unless marked nullable with `?`
- You can combine modifiers freely, e.g. `Name*!?+`

---

## 🚀 Usage

### Build (Go required)

```sh
make build
```

### Run

```sh
./tql 'User(Id, Name!, Email?)'
```

➡️ Prints the SQL to stdout  
➡️ Saves to `sql/NNNN_TableName.sql` (autonumbered)  
➡️ Prompts before overwriting if file exists

### Install for global use (on macOS/Linux)

```sh
make install
```

Then:

```sh
tql 'Company(Id, Name*, OrgNumber!+)'
```

---

## 📂 Output Example

Given:

```tql
User(Id, Name!, Email?)
```

Produces:

```sql
CREATE TABLE User (
  Id INTEGER NOT NULL PRIMARY KEY,
  Name TEXT NOT NULL UNIQUE,
  Email TEXT
);
```

Saved as `sql/0010_User.sql` or next in sequence.

---

More tooling (like IQL for inserts) will be added later.
