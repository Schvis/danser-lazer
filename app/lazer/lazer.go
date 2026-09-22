package lazer

import (
	"encoding/json"
	"fmt"
	"github.com/wieku/danser-go/app/beatmap"
	"github.com/wieku/danser-go/app/settings"
	"github.com/wieku/danser-go/app/skin"
	"github.com/wieku/danser-go/framework/env"
	"github.com/wieku/danser-go/framework/files"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
	"strings"
)

type LazerBeatmapEntry struct {
	MD5                string            `json:"MD5"`
	Hash               string            `json:"Hash"`
	OnlineID           int               `json:"OnlineID"`
	Difficulty         string            `json:"Difficulty"`
	Title              string            `json:"Title"`
	TitleUnicode       string            `json:"TitleUnicode"`
	Artist             string            `json:"Artist"`
	ArtistUnicode      string            `json:"ArtistUnicode"`
	Creator            string            `json:"Creator"`
	AudioFile          string            `json:"AudioFile"`
	BackgroundFile     string            `json:"BackgroundFile"`
	SetOnlineID        int               `json:"SetOnlineID"`
	SetHash            string            `json:"SetHash"`
	OsuFile            string            `json:"OsuFile"`
	OsuFileStoragePath string            `json:"OsuFileStoragePath"`
	StorageFiles       map[string]string `json:"StorageFiles"`
}

type LazerSkinEntry struct {
	ID           string            `json:"ID"`
	Name         string            `json:"Name"`
	Creator      string            `json:"Creator"`
	Hash         string            `json:"Hash"`
	Protected    bool              `json:"Protected"`
	StorageFiles map[string]string `json:"StorageFiles"`
}

func init() {
	settings.LazerSkinProvider = func() []string {
		entries, err := QueryLazerSkins("")
		if err != nil {
			return nil
		}
		var names []string
		for _, e := range entries {
			if len(e.StorageFiles) > 0 && e.Name != "" {
				names = append(names, e.Name)
			}
		}
		return names
	}

	skin.LazerSkinResolver = func(name string) *files.FileMap {
		entry, err := QueryLazerSkin(name)
		if err != nil || len(entry.StorageFiles) == 0 {
			return nil
		}
		return files.NewCustomFileMap("", entry.StorageFiles)
	}
}

func findBridgeExe() string {
	candidates := []string{
		filepath.Join(env.DataDir(), "bin", "lazer-bridge", "lazer-bridge.exe"),
		filepath.Join(env.DataDir(), "lazer-bridge", "lazer-bridge.exe"),
		filepath.Join(env.DataDir(), "tools", "lazer-bridge", "bin", "Release", "net8.0", "win-x64", "publish", "lazer-bridge.exe"),
		filepath.Join(env.DataDir(), "lazer-bridge.exe"),
		"lazer-bridge.exe",
	}

	for _, c := range candidates {
		if fi, err := os.Stat(c); err == nil && !fi.IsDir() {
			return c
		}
	}

	return ""
}

func QueryLazerBeatmap(md5 string, id int64) (*LazerBeatmapEntry, error) {
	bridgeExe := findBridgeExe()
	if bridgeExe == "" {
		return nil, fmt.Errorf("lazer-bridge.exe not found")
	}

	var args []string
	if md5 != "" {
		args = append(args, "--md5", md5)
	} else if id > -1 {
		args = append(args, "--id", strconv.FormatInt(id, 10))
	} else {
		return nil, fmt.Errorf("no query param provided")
	}

	cmd := exec.Command(bridgeExe, args...)
	out, err := cmd.Output()
	if err != nil {
		return nil, fmt.Errorf("failed to run lazer-bridge: %w", err)
	}

	var entries []LazerBeatmapEntry
	if err := json.Unmarshal(out, &entries); err != nil {
		return nil, fmt.Errorf("failed to parse lazer-bridge json: %w", err)
	}

	if len(entries) == 0 {
		return nil, fmt.Errorf("beatmap not found in osu!lazer database")
	}

	return &entries[0], nil
}

func LoadBeatMapFromLazer(entry *LazerBeatmapEntry) (*beatmap.BeatMap, error) {
	if entry.OsuFileStoragePath == "" {
		return nil, fmt.Errorf("osu file storage path empty for lazer beatmap")
	}

	fMap := files.NewCustomFileMap("", entry.StorageFiles)

	beatMap := beatmap.NewBeatMap()
	beatMap.Dir = ""
	beatMap.File = entry.OsuFileStoragePath
	beatMap.MD5 = entry.MD5
	beatMap.ID = int64(entry.OnlineID)
	beatMap.SetID = int64(entry.SetOnlineID)
	beatMap.SetPathCache(fMap)

	err := beatmap.ParseBeatMap(beatMap)
	if err != nil {
		return nil, fmt.Errorf("failed to parse beatmap from lazer storage: %w", err)
	}

	if entry.AudioFile != "" {
		beatMap.Audio = entry.AudioFile
	}
	if entry.BackgroundFile != "" {
		beatMap.Bg = entry.BackgroundFile
	}

	log.Printf("Loaded beatmap from osu!lazer storage: %s - %s [%s] (MD5: %s)", beatMap.Artist, beatMap.Name, beatMap.Difficulty, beatMap.MD5)
	return beatMap, nil
}

func QueryLazerSkins(filter string) ([]LazerSkinEntry, error) {
	bridgeExe := findBridgeExe()
	if bridgeExe == "" {
		return nil, fmt.Errorf("lazer-bridge.exe not found")
	}

	var args []string
	if filter != "" {
		args = append(args, "--skin", filter)
	} else {
		args = append(args, "--list-skins")
	}

	cmd := exec.Command(bridgeExe, args...)
	out, err := cmd.Output()
	if err != nil {
		return nil, fmt.Errorf("failed to run lazer-bridge: %w", err)
	}

	var entries []LazerSkinEntry
	if err := json.Unmarshal(out, &entries); err != nil {
		return nil, fmt.Errorf("failed to parse lazer-bridge skin json: %w", err)
	}

	return entries, nil
}

func QueryLazerSkin(name string) (*LazerSkinEntry, error) {
	entries, err := QueryLazerSkins(name)
	if err != nil {
		return nil, err
	}

	if len(entries) == 0 {
		return nil, fmt.Errorf("skin not found in osu!lazer database")
	}

	for _, e := range entries {
		if strings.EqualFold(e.Name, name) {
			return &e, nil
		}
	}

	return &entries[0], nil
}
