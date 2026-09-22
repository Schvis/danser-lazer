package files

import (
	"os"
	"path/filepath"
	"strings"
)

type FileMap struct {
	path      string
	pathCache map[string]string
}

func NewFileMap(path string) (*FileMap, error) {
	if _, err := os.Stat(path); os.IsNotExist(err) {
		return nil, err
	}

	fPath := strings.ReplaceAll(path, "\\", "/")
	if !strings.HasSuffix(fPath, "/") {
		fPath += "/"
	}

	fileMap := &FileMap{
		path:      fPath,
		pathCache: make(map[string]string),
	}

	results, _ := SearchFiles(fPath, "*", -1)

	for _, result := range results {
		fixedPath := strings.TrimPrefix(strings.ReplaceAll(result, "\\", "/"), fPath)
		fileMap.pathCache[strings.ToLower(fixedPath)] = fixedPath
	}

	return fileMap, nil
}

func NewCustomFileMap(basePath string, mapping map[string]string) *FileMap {
	fPath := strings.ReplaceAll(basePath, "\\", "/")
	if !strings.HasSuffix(fPath, "/") {
		fPath += "/"
	}

	fileMap := &FileMap{
		path:      fPath,
		pathCache: make(map[string]string),
	}

	for k, v := range mapping {
		fileMap.pathCache[strings.ToLower(k)] = v
	}

	return fileMap
}

func (f *FileMap) GetFile(path string) (string, error) {
	sPath := strings.ToLower(f.path)
	fPath := strings.TrimPrefix(strings.ReplaceAll(strings.ToLower(path), "\\", "/"), sPath)

	if resolved, ok := f.pathCache[fPath]; ok {
		if filepath.IsAbs(resolved) {
			return resolved, nil
		}
		return filepath.Join(f.path, resolved), nil
	}

	return "", os.ErrNotExist
}

func (f *FileMap) GetMap() map[string]string {
	retMap := make(map[string]string)

	for k, v := range f.pathCache {
		if filepath.IsAbs(v) {
			retMap[k] = v
		} else {
			retMap[k] = filepath.Join(f.path, v)
		}
	}

	return retMap
}
