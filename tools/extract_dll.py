with open(r'D:/Descargas/danser/lazer-export.exe', 'rb') as f:
    f.seek(9641984) # 0x932000
    dll_data = f.read(9216) # 0x2400

with open(r'D:/so/danser-go/tools/lazer-export.dll', 'wb') as f:
    f.write(dll_data)

print(f"Extracted lazer-export.dll! Header: {dll_data[:2]}")
