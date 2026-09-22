with open(r'D:/Descargas/danser/lazer-export.exe', 'rb') as f:
    data = f.read()

# Let's search backwards for 'lazer-export.dll'
pos = 0
while True:
    idx = data.find(b'lazer-export.dll', pos)
    if idx == -1:
        break
    print(f"Found 'lazer-export.dll' at {idx}")
    pos = idx + 1
