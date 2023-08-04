import networkx as nx

def find_unconnected_pairs(G):
    nodes = list(G.nodes())
    unconnected_pairs = []

    for i in range(len(nodes)):
        for j in range(i+1, len(nodes)):
            node1 = nodes[i]
            node2 = nodes[j]
            
            if not nx.has_path(G, node1, node2):
                unconnected_pairs.append((node1, node2))

    return unconnected_pairs