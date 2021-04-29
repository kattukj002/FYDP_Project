import re
import platform
from pathlib import Path
import os
import json
from tqdm import tqdm
home_dir = str(Path('~').expanduser())

outfile_path = "./unity_log_graphable.json"

if platform.system() == "Linux":
    log_file_path = os.path.join(home_dir, ".config/unity3d/Editor.log")
elif platform.system() == "Windows":
    log_file_path = os.path.join(home_dir, "AppData\\Local\\Unity\\Editor\\Editor.log")
elif platform.system() == "Darwin":
    log_file_path = os.path.join(home_dir, "Library/Logs/Unity/Editor.log")

log_data = {}

non_data_label_names = ["GLES", "XR"]

def extr_data(line):
    line_parts = line.split(":")
    if len(line_parts) != 2 or line_parts[0] == 'C':
        return
    else:
        label = line_parts[0]
        
        data = line_parts[1]
        label_alpha = re.sub('[^a-zA-Z]+', '', label)
        if not label.isupper() or ' ' in label or label in non_data_label_names:
            return
        else:
            if ("(" in data):
                if label not in log_data.keys():
                    log_data[label] = {
                        'x' : [],
                        'y' : [],
                        'z' : []
                    }
                vector_parts = re.sub('[^\-0-9\.\,]+', '', data).split(',')
                for i in range(len(vector_parts)):
                    vector_parts[i] = re.sub('[-\.]+$', '', vector_parts[i])
                try:
                    log_data[label]['x'].append(safe_to_float(vector_parts[0]))
                    log_data[label]['y'].append(safe_to_float(vector_parts[1]))
                    log_data[label]['z'].append(safe_to_float(vector_parts[2]))
                    
                except:
                    import pdb;pdb.set_trace()
            else:
                if label not in log_data.keys():
                    log_data[label] = []
                
                log_data[label].append(safe_to_float(re.sub('[^0-9\.]+', '', data)))
                

def safe_to_float(string):
    if string == "" or  re.sub('[^0-9]+', '', string) == "":
        return -1 
    else:
        try:
            return float(string)
        except:
            import pdb;pdb.set_trace()
    
log_file = open(log_file_path, 'r')

for line in tqdm(log_file):
    extr_data(line)
        
with open(outfile_path, 'w') as outfile:
    outfile.write(json.dumps(log_data, indent=2))
