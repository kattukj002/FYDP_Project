import matplotlib.pyplot as plt
import numpy as np
import json
from copy import deepcopy
import math
fmts = ['b.-','g.-', 'r.-', 'c.-', 'm.-', 'y.-', 'k.-']
smooth_fmts = ['b-','g-', 'r-', 'c-', 'm-', 'y-', 'k-']

ax_idx = {
    'x': 0,
    'y': 1,
    'z': 2
}

def graphable(filename='./unity_log_graphable.json', window_size=1):
    graphable_dict = json.loads(open(filename, 'r').read())
    
    for k in graphable_dict.keys():
        if type(graphable_dict[k]) == type({}):
            graphable_dict[k] = np.stack(
                 [moving_avg(np.array(graphable_dict[k]['z']), window_size),
                  moving_avg(-np.array(graphable_dict[k]['x']), window_size),
                  moving_avg(np.array(graphable_dict[k]['y']), window_size)], axis=0)
        else:
            graphable_dict[k] = moving_avg(np.array(graphable_dict[k]), window_size)

    return graphable_dict

def graphable_vars_to_lists(graphable_dict):
    for k in graphable_dict.keys():
        graphable_dict[k] = graphable_dict[k].tolist()

def graphable_vars_to_nparrays(graphable_dict):
    for k in graphable_dict.keys():
        graphable_dict[k] = np.array(graphable_dict[k])

def plot_all(filename='./unity_log_graphable.json',
            time_lims=None, period=100):
    graphable_dict = graphable(filename)
    plot_joint_geom(graphable_dict, time_lims=time_lims, period=period, show_plot=False)
    plot_lengths(graphable_dict, time_lims=time_lims, show_plot=False)
    plot_torques(graphable_dict, time_lims=time_lims, show_plot=False)
    plot_angles(graphable_dict, time_lims=time_lims, show_plot=False)
    plot_calced_vectors(graphable_dict, time_lims=time_lims, show_plot=False)
    plt.show()

def plot_joint_geom(graphable_dict, time_lims=None, period=100, show_plot=True):
    if time_lims == None:
        time_lims = default_time_lims(graphable_dict)

    #posn_key_list = ['HEAD_POSITION', 'NECK_BASE_POSITION', 
    posn_key_list = ['SHOULDER_POSITION',
                'ELBOW_POSITION', 'HAND_POSITION']

    coords = [graphable_dict[k] for k in posn_key_list]
    ax = setup_3d_plot()
    ax.set_box_aspect(max_range_box(coords))
    
    for i in range(len(posn_key_list)):
        if i < len(posn_key_list) - 1:
            plot_connecting_vectors(ax, coords[i], coords[i+1], 
                                   time_lims, fmts[i], period=period)

        plot_positions(ax, coords[i], time_lims, fmts[i], posn_key_list[i]) 

        

    axis_vectors = {
        'SHOULDER_RIGHT_AXIS_VECTOR': 'SHOULDER_POSITION', 
        'TORSO_FORWARD_AXIS_VECTOR' : 'SHOULDER_POSITION', 
        'TORSO_UP_AXIS_VECTOR' : 'SHOULDER_POSITION', 
        'RIGHT_ELBOW_AXIS_VECTOR' : 'ELBOW_POSITION'
    }
    ax_count = 0
    for axis_name, start_vec_name in axis_vectors.items():
        
        vec_ends = graphable_dict[start_vec_name] + normalize(graphable_dict[axis_name]) * 0.1
        
        # if 'TORSO_UP_AXIS_VECTOR' == axis_name:
        #     tmp = np.linalg.norm(normalize(graphable_dict[axis_name]), axis=0)
        #     import pdb;pdb.set_trace()

        # plot_positions(ax, vec_ends, time_lims, fmts[ax_count] + "-", label=axis_name) 
        
        # plot_connecting_vectors(ax, graphable_dict[start_vec_name], 
        #                             vec_ends, 
        #                             time_lims, fmts[ax_count] + "-", period=period)
        ax_count += 1
    ax.set_title('Arm geometry, interpolated arm vectors')
    ax.legend(loc='upper right')
    if show_plot:
        plt.show()

def normalize(vectors):
    vec_copy = deepcopy(vectors)
    norms = np.linalg.norm(vec_copy, axis=0)
    nonzero_norms =  norms != 0
    vec_copy[:, nonzero_norms] = vec_copy[:, nonzero_norms] / norms[nonzero_norms]
    return vec_copy

def plot_lengths(graphable_dict, time_lims=None, show_plot=True):
    if time_lims == None:
        time_lims = default_time_lims(graphable_dict)  
    length_keys = {
        'SHOULDER_TO_HAND_LENGTH': 'SHOULDER_MOMENT_ARM_VECTOR'
    }
    fig, axes = plt.subplots(len(length_keys), 1)
    for i, key in enumerate(length_keys):
        axes.set_xlabel("Update Num")
        axes.set_ylabel("Length [m]")
        plot_scalar(axes, graphable_dict[key], time_lims, fmts[0], label="Logged Length")
        plot_scalar(axes, np.linalg.norm(graphable_dict[length_keys[key]], axis=0), 
                    time_lims, fmts[1], label="Interpolated Length")
        axes.set_title(key)
        adjust_ax_ylims(axes)
        axes.legend(loc='upper right')
        axes.grid(True)
    fig.subplots_adjust(hspace=0.4)
    if show_plot:
        plt.show()
  
def plot_torques(graphable_dict, time_lims=None, show_plot=True):
    if time_lims == None:
        time_lims = default_time_lims(graphable_dict)   
    torque_keys = ['ELBOW_TORQUE', 'SHOULDER_ABDUCTION_TORQUE', 'SHOULDER_FLEXION_TORQUE']
    
    fig, axes = plt.subplots(len(torque_keys), 1)
     
    for i, key in enumerate(torque_keys):
        axes[i].set_xlabel("Update Num")
        axes[i].set_ylabel("Torque [N-m]")
        plot_scalar(axes[i], graphable_dict[key], time_lims, fmts[i])
        axes[i].set_title(key)
        axes[i].grid(True)
        adjust_ax_ylims(axes[i])
    fig.subplots_adjust(hspace=0.4)
    if show_plot:
        plt.show()

def plot_angles(graphable_dict, time_lims=None, show_plot=True):
    if time_lims == None:
        time_lims = default_time_lims(graphable_dict)

    angles_from_vectors = {
        'NECK_BASE_HAND_NECK_BASE_SHOULDER_ANGLE' : 
            (graphable_dict['NECK_BASE_TO_HAND'], 
            graphable_dict['NECK_BASE_TO_SHOULDER']),
        'ELBOW_ANGLE' :
            (-graphable_dict['UPPER_ARM_VECTOR'], 
            graphable_dict['LOWER_ARM_VECTOR'])
    }

    angles_from_projections = {
        'SHOULDER_ABDUCTION_ANGLE' : {
            'orig_vector' : graphable_dict['UPPER_ARM_VECTOR'],
            'proj_plane' : graphable_dict['TORSO_FORWARD_AXIS_VECTOR'],
            'start_vec' :  -graphable_dict['TORSO_UP_AXIS_VECTOR']
        },
        'SHOULDER_FLEXION_ANGLE' : {
            'orig_vector' : graphable_dict['UPPER_ARM_VECTOR'],
            'proj_plane' : graphable_dict['SHOULDER_RIGHT_AXIS_VECTOR'],
            'start_vec' :  -graphable_dict['TORSO_UP_AXIS_VECTOR']
        } 
    }

    fig, axes = plt.subplots(len(angles_from_vectors.keys()) + len(angles_from_projections.keys()), 1)

    ax_count = 0
    for angle_name, interp_vectors in angles_from_vectors.items():
        interp_angles = np.zeros((interp_vectors[0].shape[1]))

        for i in range(len(interp_angles)):
            
            interp_angles[i] = lt_180_angle(interp_vectors[0][:, i], interp_vectors[1][:, i])
            if angle_name == 'ELBOW_ANGLE':
                pass
                #import pdb;pdb.set_trace()
            
        axes[ax_count].set_xlabel("Update Num")
        axes[ax_count].set_ylabel("Angle [deg]")

        plot_scalar(axes[ax_count], interp_angles, time_lims, fmts[0], "Interpolated Angle")
        plot_scalar(axes[ax_count], graphable_dict[angle_name], time_lims, fmts[1], "Logged Angle")
        
        axes[ax_count].set_title(angle_name)
        axes[ax_count].grid(True)
        axes[ax_count].legend(loc='upper right')
        adjust_ax_ylims(axes[ax_count])
        ax_count += 1

    for angle_name, proj_vector_dict in angles_from_projections.items():
        
        project_angles = np.zeros((proj_vector_dict['orig_vector'].shape[1]))

        for i in range(len(project_angles)):
            project_angles[i] = lt_180_angle(
                proj_vector_dict['start_vec'][:, i],
                proj_on_plane(
                    proj_vector_dict['orig_vector'][:, i], 
                    normal=proj_vector_dict['proj_plane'][:, i])
            )
        
        axes[ax_count].set_xlabel("Update Num")
        axes[ax_count].set_ylabel("Angle [deg]")
        
        plot_scalar(axes[ax_count], project_angles, time_lims, fmts[0], "Interpolated Angle")
        plot_scalar(axes[ax_count], graphable_dict[angle_name], time_lims, fmts[1], "Logged Angle")

        axes[ax_count].set_title(angle_name)
        axes[ax_count].grid(True)
        axes[ax_count].legend(loc='upper right')
        adjust_ax_ylims(axes[ax_count])
        ax_count += 1

    fig.subplots_adjust(hspace=0.5)
    if show_plot:
        plt.show()             

def plot_calced_vectors(graphable_dict, time_lims=None, period=100, show_plot=True):
    if time_lims == None:
        time_lims = default_time_lims(graphable_dict)   
    
    vectors = {
        'NECK_BASE_TO_HAND': {
            'start': 'NECK_BASE_POSITION', 
            'end': 'HAND_POSITION'
        }, 
        'NECK_BASE_TO_SHOULDER': {
            'start': 'NECK_BASE_POSITION', 
            'end': 'SHOULDER_POSITION'
        }, 
        'UPPER_ARM_VECTOR': {
            'start': 'SHOULDER_POSITION', 
            'end': 'ELBOW_POSITION'
        }, 
        'SHOULDER_MOMENT_ARM_VECTOR': {
            'start': 'SHOULDER_POSITION', 
            'end': 'HAND_POSITION'
        }, 
        'LOWER_ARM_VECTOR': {
            'start': 'ELBOW_POSITION', 
            'end': 'HAND_POSITION'
        }
    }
    for curr_vec in vectors.keys():
        ax = setup_3d_plot()

        vector_ends = graphable_dict[vectors[curr_vec]['start']] + graphable_dict[curr_vec]
        
        plot_connecting_vectors(ax, graphable_dict[vectors[curr_vec]['start']], vector_ends, 
                                time_lims, fmts[0], 
                                label=curr_vec, period=period)
        
        plot_positions(ax, graphable_dict[vectors[curr_vec]['start']], time_lims, fmts[1], 
                        label=vectors[curr_vec]['start'])
        plot_positions(ax, graphable_dict[vectors[curr_vec]['end']], time_lims, fmts[2], 
                        label=vectors[curr_vec]['end'])
        
        ax.set_box_aspect(max_range_box([
            vector_ends, 
            graphable_dict[vectors[curr_vec]['start']], 
            graphable_dict[vectors[curr_vec]['end']]
        ]))

        ax.legend(loc='upper right')
        ax.set_title(curr_vec)
    if show_plot:
        plt.show()

def rad_to_deg(angle):
    return angle * 180 / math.pi
def lt_180_angle(vec_1, vec_2):
    return rad_to_deg(
        math.acos(np.dot(vec_1, vec_2) / np.linalg.norm(vec_1) / np.linalg.norm(vec_2))
    )

def proj_on_vec(from_vec, to_vec):
    return np.dot(from_vec, to_vec) / np.linalg.norm(to_vec) ** 2 * to_vec

def proj_on_plane(vec, normal):
    return vec - proj_on_vec(vec, normal)

def default_time_lims(graphable_dict):
    return (0, graphable_dict['ELBOW_POSITION'].shape[1])

def setup_3d_plot():
    fig = plt.figure()
    ax = plt.axes(projection='3d')
    ax.set_xlabel('x')
    ax.set_ylabel('y')
    ax.set_zlabel('z')
    return ax

def adjust_ax_ylims(ax, margin=0.1):
    bottom, top = ax.get_ylim()
    new_bottom = min(bottom, 0)
    new_top = max(top, 0)
    y_range = new_top - new_bottom
    ax.set_ylim(bottom=new_bottom - y_range * margin, 
                top=new_top + y_range * margin)

def max_range_box(coords_list):
    mins = []
    maxes = []

    for coords in coords_list:
        temp_mins = np.amin(coords, axis=1)
        temp_maxes = np.amax(coords, axis=1)
        
        if len(mins) == 0:
            mins = temp_mins
            maxes = temp_maxes
        else:
            repl_mins = temp_mins < mins
            repl_maxes = temp_maxes > maxes
            mins[repl_mins] = temp_maxes[repl_mins]
            maxes[repl_maxes] = temp_maxes[repl_maxes]
    
    #return maxes - mins
    tr_max = max(maxes - mins)
    return (tr_max, tr_max, tr_max)

def plot_scalar(ax, values, time_lims, fmt, label="_"):
    ax.plot(np.arange(time_lims[0], time_lims[1]), values[time_lims[0]: time_lims[1]], fmt, label=label)

def plot_positions(ax, coords, time_lims, fmt, label="_", period=1):
    ax.plot3D(coords[ax_idx['x'], time_lims[0]: time_lims[1]:period], 
              coords[ax_idx['y'], time_lims[0]: time_lims[1]:period], 
              coords[ax_idx['z'], time_lims[0]: time_lims[1]:period], 
              fmt, label=label)

def plot_connecting_vectors(ax, coords_1, coords_2, time_lims, fmt, label="_", period=1):
    ax.plot3D([coords_1[ax_idx['x'], time_lims[0]],coords_2[ax_idx['x'], time_lims[0],]], 
            [coords_1[ax_idx['y'], time_lims[0]], coords_2[ax_idx['y'], time_lims[0],]], 
            [coords_1[ax_idx['z'], time_lims[0]], coords_2[ax_idx['z'], time_lims[0],]],
            fmt, label=label)
    for i in range(time_lims[0] + 1, time_lims[1], period):
        ax.plot3D([coords_1[ax_idx['x'], i],coords_2[ax_idx['x'], i]], 
              [coords_1[ax_idx['y'], i], coords_2[ax_idx['y'], i]], 
              [coords_1[ax_idx['z'], i], coords_2[ax_idx['z'], i]],
              fmt, label="_")

def moving_avg(data, window_size):
    if window_size > 1:
        if len(data.shape) > 1:
            max_data_cols = data.shape[1]
            data_copy = np.zeros((data.shape[0], data.shape[1] - (window_size - 1)))

            for i in range(window_size - 1, max_data_cols):
                data_copy[:, i - (window_size - 1)] = np.mean(data[:, (i + 1 - window_size):(i + 1)], axis=1)
        else:
            max_data_cols = data.shape[0]

            data_copy = np.zeros((data.shape[0] - (window_size - 1)))
            
            for i in range(window_size - 1, max_data_cols):
                data_copy[i - (window_size - 1)] = np.mean(data[(i + 1 - window_size):(i + 1)])
    else:
        data_copy = deepcopy(data)

    return data_copy