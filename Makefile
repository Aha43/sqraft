SOLUTION := sqraft.sln
SRC_DIR := src
INSTALL_DIR := ~/.dotnet/tools

# Default: build everything
all:
	dotnet build $(SOLUTION)

test:
	dotnet test $(SOLUTION)

# Build a specific project (ex: make build PROJECT=Tools/Tool1)
#build:
#	dotnet build $(SRC_DIR)/$(PROJECT)

# Install a console tool globally (assumes it's a .NET tool)
#install:
#	dotnet publish $(SRC_DIR)/$(PROJECT) -c Release -o $(INSTALL_DIR)

# Clean all
clean:
	dotnet clean $(SOLUTION)

# Run a console project (ex: make run PROJECT=Tools/Tool1 ARGS="arg1 arg2")
run:
	dotnet run --project $(SRC_DIR)/$(PROJECT) -- $(ARGS)

# Format all projects using dotnet format
format:
	dotnet format $(SOLUTION)
