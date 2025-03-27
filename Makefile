APP_NAME = tql
SRC_DIR = ./cmd/sqraft
OUT_DIR = ./bin
BIN_PATH = $(OUT_DIR)/$(APP_NAME)

# Default target
all: build

# Build binary
build:
	go build -o $(BIN_PATH) $(SRC_DIR)

# Run with example input (handy for dev testing)
run:
	go run $(SRC_DIR) 'User(Id, Name!, Email?)'

# Install binary to local bin (macOS/Linux-style)
install: build
	cp $(BIN_PATH) $(HOME)/.local/bin/$(APP_NAME)

# Clean build artifacts
clean:
	rm -f $(BIN_PATH)

.PHONY: all build run install clean
