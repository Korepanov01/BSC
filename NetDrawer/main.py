import json
import igraph
import cairo

parameters_names = {0 : 'ИТ', 1 : 'Финансы', 2 : 'Клиенты', 3 : 'Внутр. процессы', 4 : 'Обучение, рост'}


def get_label(node):
    label = node['Description']

    for i, parameters in enumerate(node['Parameters']):
        if parameters:
            label += f'\n{parameters_names[i]}: {sum((parameter["Value"] for parameter in parameters))}'

    return label


tree_data_file = open('net.json', 'r', encoding='utf-8')
tree_data = json.load(tree_data_file)

nodes = tree_data["Item1"]
edges = [(pair["From"], pair["To"]) for pair in tree_data["Item2"]]

graph = igraph.Graph(directed=True)
graph.add_vertices(len(nodes))
graph.add_edges(edges)
graph.degree(mode="in")

graph.vs['label'] = [get_label(node) for node in nodes]

igraph.plot(graph,
            bbox=(1280, 720),
            margin=150,

            vertex_shape='rectangle',
            vertex_width=170,
            vertex_height=120,
            vertex_color='white',

            #edge_arrow_size=7,
            # vertex_color=['red' if node["RiskLevel"] > 15 else
            #               'yellow' if node["RiskLevel"] > 5 else
            #               'green' if node["RiskLevel"] > 0 else 'white'
            #               for node in nodes],

            # layout=graph.layout_reingold_tilford(mode="in", root=0)
            )
