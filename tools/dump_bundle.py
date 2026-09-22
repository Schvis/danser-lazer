import struct
import os

with open(r'D:/Descargas/danser/lazer-export.exe', 'rb') as f:
    data = f.read()

# Let's find bundle header:
# In .NET 6/7/8 single-file, bundle manifest is pointed to by a 32-byte GUID or near the end.
# Look at the filenames at the end of the file:
# mscorlib.dll, netstandard.dll, Realm.dll, lazer-export.deps.json...
# Let's see the structure around the end:
print("Length of file:", len(data))
print("Last 1000 bytes:")
print(data[-1000:])
