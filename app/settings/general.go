package settings

import (
	"github.com/wieku/danser-go/framework/env"
	"path/filepath"
)

var General = initGeneral()

func initGeneral() *general {
	osuBaseDir := getOsuInstallation()

	return &general{
		UseLazer:          false,
		OsuSongsDir:       filepath.Join(osuBaseDir, "Songs"),
		OsuSkinsDir:       filepath.Join(osuBaseDir, "Skins"),
		OsuReplaysDir:     filepath.Join(osuBaseDir, "Replays"),
		DiscordPresenceOn: true,
		UnpackOszFiles:    true,
		VerboseImportLogs: false,
	}
}

type general struct {
	// Use osu!lazer paths instead of osu!stable
	UseLazer bool `label:"Use osu!lazer paths"`

	// Directory that contains osu! songs
	OsuSongsDir string `long:"true" label:"osu! Songs directory" path:"Select osu! Songs directory"`

	// Directory that contains osu! skins
	OsuSkinsDir string `long:"true" label:"osu! Skins directory" path:"Select osu! Skins directory"`

	// Directory that contains osu! replays
	OsuReplaysDir string `long:"true" label:"osu! Replays directory" path:"Select osu! Replays directory" tooltip:"Don't use replays directory inside danser's directory!"`

	// Whether discord should show that danser is on
	DiscordPresenceOn bool `label:"Discord Rich Presence"`

	// Whether danser should unpack .osz files in Songs folder, osu! may complain about it
	UnpackOszFiles bool

	// Whether import details should be shown. If false, only failures will be logged.
	VerboseImportLogs bool

	songsDir   *string
	skinsDir   *string
	replaysDir *string
}

func (g *general) GetSongsDir() string {
	if g.songsDir == nil {
		dir := filepath.Join(env.DataDir(), g.OsuSongsDir)

		if filepath.IsAbs(g.OsuSongsDir) {
			dir = g.OsuSongsDir
		}

		g.songsDir = &dir
	}

	return *g.songsDir
}

func (g *general) GetSkinsDir() string {
	if g.skinsDir == nil {
		dir := filepath.Join(env.DataDir(), g.OsuSkinsDir)

		if filepath.IsAbs(g.OsuSkinsDir) {
			dir = g.OsuSkinsDir
		}

		g.skinsDir = &dir
	}

	return *g.skinsDir
}

func (g *general) GetReplaysDir() string {
	if g.replaysDir == nil {
		dir := filepath.Join(env.DataDir(), g.OsuReplaysDir)

		if filepath.IsAbs(g.OsuReplaysDir) {
			dir = g.OsuReplaysDir
		}

		g.replaysDir = &dir
	}

	return *g.replaysDir
}

func GetOsuInstallation() string {
	return getOsuInstallation()
}

func GetLazerInstallation() string {
	return getLazerInstallation()
}

func (g *general) InvalidateCache() {
	g.songsDir = nil
	g.skinsDir = nil
	g.replaysDir = nil
}

func (g *general) SwitchToLazer(lazer bool) {
	g.UseLazer = lazer
	if lazer {
		baseDir := GetLazerInstallation()
		g.OsuSongsDir = baseDir
		g.OsuSkinsDir = baseDir
		g.OsuReplaysDir = filepath.Join(baseDir, "exports")
	} else {
		baseDir := GetOsuInstallation()
		g.OsuSongsDir = filepath.Join(baseDir, "Songs")
		g.OsuSkinsDir = filepath.Join(baseDir, "Skins")
		g.OsuReplaysDir = filepath.Join(baseDir, "Replays")
	}
	g.InvalidateCache()
}
