import graph_unity_log as gul 
from graph_unity_log import *
import matplotlib.pyplot as plt 
import numpy as np
from copy import deepcopy
import json

period = 100
window_size = 10

new_json = True
avg_json_file = './unity_log_avged_graphable.json' 

if new_json:
    graphable_dict = gul.graphable(window_size=window_size)
    with open(avg_json_file, 'w') as f:
        save_dict = deepcopy(graphable_dict)
        gul.graphable_vars_to_lists(save_dict)
        json.dump(save_dict, f)
        del save_dict
else:
    graphable_dict = json.load(open(avg_json_file, 'r'))
    gul.graphable_vars_to_nparrays(graphable_dict)

time_lims = gul.default_time_lims(graphable_dict)
#import pdb;pdb.set_trace()
#gul.plot_joint_geom(graphable_dict, time_lims=time_lims)
'''
posn_key_list = ['SHOULDER_POSITION', 'ELBOW_POSITION', 'HAND_POSITION']
coords = [graphable_dict[k] for k in posn_key_list]

for i in range(len(coords)):
    too_big = np.abs(coords[i]) > 10
    coords[i][too_big] = -1

ax = gul.setup_3d_plot()
ax.set_box_aspect(gul.max_range_box(coords))

for i in range(len(posn_key_list)):

    gul.plot_positions(ax, gul.moving_avg(coords[i], window_size), 
        time_lims, gul.smooth_fmts[i + 2], posn_key_list[i]) 

    gul.plot_positions(ax, gul.moving_avg(coords[i], window_size), 
        (time_lims[0] + 1, time_lims[1]), gul.fmts[i][0:-1], posn_key_list[i], period=period) 

    gul.plot_positions(ax, gul.moving_avg(coords[i], window_size), 
        [time_lims[0],time_lims[0] + 1], gul.fmts[i][0] + "x", posn_key_list[i] + "_START")
    

ax.set_title('Arm geometry, interpolated arm vectors')
'''


ax = gul.setup_3d_plot()

# ax.plot3D([graphable_dict['UPPER_ARM_VECTOR'][0, 1], 0], 
#               [graphable_dict['UPPER_ARM_VECTOR'][1, 1], 0], 
#               [graphable_dict['UPPER_ARM_VECTOR'][2, 1], 0], 
#               'r.--', label='_')

#gul.plot_positions(ax, graphable_dict['UPPER_ARM_VECTOR'], time_lims, gul.fmts[1], label="UPPER_ARM_VECTOR", period=100)

# ax.plot3D([graphable_dict['LOWER_ARM_VECTOR'][0, 1], 0], 
#               [graphable_dict['LOWER_ARM_VECTOR'][1, 1], 0], 
#               [graphable_dict['LOWER_ARM_VECTOR'][2, 1], 0], 
#               'r.--', label='_')
# gul.plot_positions(ax, graphable_dict['LOWER_ARM_VECTOR'], time_lims, gul.fmts[1], label="LOWER_ARM_VECTOR")

# gul.plot_connecting_vectors(ax, np.zeros((3, time_lims[1])), graphable_dict['LOWER_ARM_VECTOR'], 
#         time_lims, 'r.--', period=100)

#gul.plot_positions(ax, graphable_dict['LOWER_ARM_VECTOR'], time_lims, 'g-', label="LOWER_ARM_VECTOR")
# gul.plot_positions(ax, graphable_dict['ELBOW_POSITION'], time_lims, 'r-', label="ELBOW_POSITION", period=1)
gul.plot_positions(ax, graphable_dict['HAND_POSITION'], time_lims, 'b-', label="HAND_POSITION", period=1)

ax.set_box_aspect(gul.max_range_box([graphable_dict['HAND_POSITION']]))
ax.legend()
ax.set_title("Right Controller Readout")
plt.show()
'''
fig = plt.figure()
ax = plt.subplot(3,1,1)
plt.plot(np.arange(0, len(graphable_dict['ELBOW_CMD_TORQUE'])), graphable_dict['ELBOW_CMD_TORQUE'], 'r-', label="ELBOW_CMD_TORQUE")
plt.plot(np.arange(0, len(graphable_dict['ELBOW_TORQUE'])), graphable_dict['ELBOW_TORQUE'], 'b-', label="ELBOW_TORQUE")
ax.legend(loc='upper right')
plt.grid(True)

ax = plt.subplot(3,1,2)
plt.plot(np.arange(0, len(graphable_dict['ELBOW_CMD_ID'])), graphable_dict['ELBOW_CMD_ID'], 'r-', label="ELBOW_CMD_ID")
ax.legend(loc='upper right')
plt.grid(True)

ax = plt.subplot(3,1,3)
plt.plot(np.arange(0, len(graphable_dict['ELBOW_DEG_VELOCITY'])), graphable_dict['ELBOW_DEG_VELOCITY'], 'r-', label="ELBOW_DEG_VELOCITY")
ax.legend(loc='upper right')
plt.grid(True)
'''
'''
ax = plt.subplot(2,1,1)
plt.plot(np.arange(time_lims[0], time_lims[1]), graphable_dict['ELBOW_ANGLE'][time_lims[0]: time_lims[1]], 'r-', label="ELBOW_ANGLE")
ax.set_yticks(range(0, 720, 90))
ax = plt.subplot(2,1,2)
plt.plot(np.arange(time_lims[0], time_lims[1]), graphable_dict['ELBOW_DEG_VELOCITY'][time_lims[0]: time_lims[1]], 'b-', label="ELBOW_DEG_VELOCITY")
ax.legend(loc='upper right')
'''
'''
fig = plt.figure()
ax = plt.gca()
plot_scalar(ax, graphable_dict['ELBOW_ANGLE'], time_lims, 'k-', label="Elbow Angle")
ax.legend(loc='upper right')
plt.grid(True)
plt.show()
'''
