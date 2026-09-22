import struct

with open(r'D:/Descargas/danser/lazer-export.exe', 'rb') as f:
    data = f.read()

# Let's inspect bytes around 68606461
idx = 68606461
chunk = data[idx-30:idx+60]
print("Around 68606461:")
print(chunk)
print(chunk.hex())
