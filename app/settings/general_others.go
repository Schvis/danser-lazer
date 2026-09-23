//go:build !windows

package settings

import (
	"os"
	"path/filepath"
)

func getOsuInstallation() string {
	dir, _ := os.UserHomeDir()
	return filepath.Join(dir, ".osu")
}

func getLazerInstallation() string {
	dir, _ := os.UserHomeDir()
	return filepath.Join(dir, ".local", "share", "osu")
}
