import os
import shutil

osu_dir = "C:/Users/Schvis/AppData/Roaming/osu"
temp_dir = "C:/Users/Schvis/AppData/Roaming/osu_temp"
recycle_dir = "C:/$Recycle.Bin/S-1-5-21-2756535518-281313342-3977316583-1000"

# Put recycle bin items back to Recycle.Bin if possible, or clean up
for item in os.listdir(osu_dir):
    src = os.path.join(osu_dir, item)
    try:
        dst = os.path.join(recycle_dir, item)
        shutil.move(src, dst)
    except Exception as e:
        print(f"Cleanup {item}: {e}")

# If osu_dir is now empty, remove it or move temp contents into it
for item in os.listdir(temp_dir):
    src = os.path.join(temp_dir, item)
    dst = os.path.join(osu_dir, item)
    shutil.move(src, dst)

os.rmdir(temp_dir)
print("Restored cleanly to C:/Users/Schvis/AppData/Roaming/osu!")
print("Current osu contents:", os.listdir(osu_dir))
