import os

osu_dir = "C:/Users/Schvis/AppData/Roaming/osu"
for item in os.listdir(osu_dir):
    if item.startswith("$") or item == "desktop.ini":
        p = os.path.join(osu_dir, item)
        try:
            if os.path.isdir(p):
                os.rmdir(p)
            else:
                os.remove(p)
            print(f"Removed recycled item: {item}")
        except Exception as e:
            print(f"Error removing {item}: {e}")

print("Clean osu dir:", os.listdir(osu_dir))
