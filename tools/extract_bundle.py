import struct

with open(r'D:/Descargas/danser/lazer-export.exe', 'rb') as f:
    data = f.read()

# Let's see the single file header structure
# In .NET bundle format (SingleFileHost):
# The bundle header offset is stored at the end or searched.
# Let's inspect the last 64 bytes carefully:
print("Length:", len(data))
print("Hex of last 64 bytes:", data[-64:].hex())
