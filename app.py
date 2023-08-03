import typer
import os
from PyPDF2 import PdfReader
from transformers import AutoTokenizer, AutoModelForTokenClassification
from transformers import pipeline

tokenizer = AutoTokenizer.from_pretrained("RJuro/SciNERTopic")
model_trf = AutoModelForTokenClassification.from_pretrained("RJuro/SciNERTopic")

nlp = pipeline("ner", model=model_trf, tokenizer=tokenizer, aggregation_strategy='average')
app = typer.Typer()


@app.command()
def generate():
    # Check to see if a .litmap/ directory exists
    # Else make a new directory to store litmap files called .litmap/
    if not os.path.exists(".litmap/"):
        os.mkdir(".litmap/")
    
    # Iterate through current directory and find all pdf files
    for file in os.listdir():
        if file.endswith(".pdf"):
            with open(file, 'rb') as file:
                reader = PdfReader(file)

                for page in reader.pages:
                    print(page.extract_text())
                


@app.command()
def report(name: str, formal: bool = False):
    if formal:
        print(f"Goodbye Ms. {name}. Have a good day.")
    else:
        print(f"Bye {name}!")


if __name__ == "__main__":
    app()