from helpers import find_unconnected_pairs
import typer
import os
import networkx as nx
from pprint import pprint
from PyPDF2 import PdfReader
from transformers import AutoTokenizer, AutoModelForTokenClassification
from transformers import pipeline

tokenizer = AutoTokenizer.from_pretrained("RJuro/SciNERTopic", model_max_length=512)
model_trf = AutoModelForTokenClassification.from_pretrained("RJuro/SciNERTopic")

nlp = pipeline("ner", model=model_trf, tokenizer=tokenizer, aggregation_strategy='average')
app = typer.Typer()

graph_path = ".litmap/research_graph.xml"


@app.command()
def generate():
    # Check to see if a .litmap/ directory exists
    # Else make a new directory to store litmap files called .litmap/
    if not os.path.exists(".litmap/"):
        os.mkdir(".litmap/")

    # Create the graph
    G = nx.Graph()
    paperNodes = []
    
    # Iterate through current directory and find all pdf files
    for file_name in os.listdir():
        # TODO: Maybe support .txt, .docx, and .md files?
        # Check to see if the file is a pdf
        if file_name.endswith(".pdf"):
            with open(file_name, 'rb') as fp:
                reader = PdfReader(fp)
                word_label_pairs = []

                # Create a paper node
                G.add_node(file_name, type="paper")
                paperNodes.append(file_name)

                for page in reader.pages:
                    # TODO: Clean the text
                    text = page.extract_text()
                    results = nlp(text)

                    for result in results:
                        word_label_pairs.append((result['word'], result['entity_group']))
                
                # Add the nodes to the graph and connect them to the paper node
                for word_label_pair in word_label_pairs:
                    word = word_label_pair[0]
                    label = word_label_pair[1]

                    # Check to see if the node already exists
                    if not G.has_node(word):
                        G.add_node(word, type=label, name=word)

                    G.add_edge(file_name, word)

    # Connect all the paper nodes together
    for i in range(len(paperNodes)):
        for j in range(i+1, len(paperNodes)):
            node1 = paperNodes[i]
            node2 = paperNodes[j]
            G.add_edge(node1, node2)

    # Enrich the graph with extra information

    # Save the graph as a pickle
    nx.write_graphml(G, graph_path)    


@app.command()
def report():
    # Checking if there is an exisiting graph
    if not os.path.exists(graph_path):
        print("No graph found. Please run `litmap generate` to generate a graph.")
        return
    
    # Load the graph
    G = nx.read_sparse6(graph_path)

    print("Graph Report")
    print("Number of nodes: ", G.number_of_nodes())
    # Grabbing pagerank results
    pagerank_results = nx.pagerank(G)
    
    # Sort the pagerank results
    pagerank_results = sorted(pagerank_results.items(), key=lambda x: x[1], reverse=True)

    # Print the top 10 pagerank results
    print("Top 10 pagerank results:")
    pprint(pagerank_results[:10])

    # Link prediction over unconnected pairs
    # Grab the unconnected pairs of nodes
    unconnected_pairs = find_unconnected_pairs(G)

    # Predict the links
    predicted_links = []
    for pair in unconnected_pairs:
        predicted_links.append((pair, nx.jaccard_coefficient(G, [pair])[0][2]))

    # Sort the predicted links
    predicted_links.sort(key=lambda x: x[1], reverse=True)

    # Print the top 10 predicted links
    print("Top 10 predicted links:")
    pprint(predicted_links[:10])






if __name__ == "__main__":
    app()