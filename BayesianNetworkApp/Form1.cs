using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BayesianNetworkApp
{
    public partial class Form1 : Form
    {
        private Dictionary<string, Node> Nodes = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // legatura intre butoane si evenimente
            loadButton.Click += LoadButton_Click;
            setEvidenceButton.Click += SetEvidenceButton_Click;
            queryButton.Click += QueryButton_Click; //TODO
        }


        private void LoadButton_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Select Bayesian Network XML"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadNetwork(openFileDialog.FileName);
                MessageBox.Show("Network loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadNetwork(string filePath)
        {
            Nodes.Clear();

            var doc = XDocument.Load(filePath);
            var network = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "NETWORK");

            // parsarea nodurilor
            var variables = network.Descendants().Where(e => e.Name.LocalName == "VARIABLE");
            foreach (var variable in variables)
            {
                var name = variable.Descendants().First(e => e.Name.LocalName == "NAME").Value;
                var outcomes = variable.Descendants()
                    .Where(e => e.Name.LocalName == "OUTCOME")
                    .Select(e => e.Value).ToList();

                Nodes[name] = new Node { Name = name, Outcomes = outcomes };
            }

            queryNodeComboBox.Items.Clear();
            queryNodeComboBox.Items.AddRange(Nodes.Keys.ToArray());
            queryNodeComboBox.SelectedIndex = 0;

            // parsarea definitiilor
            var definitions = network.Descendants().Where(e => e.Name.LocalName == "DEFINITION");
            foreach (var definition in definitions)
            {
                var forNode = definition.Descendants().First(e => e.Name.LocalName == "FOR").Value;
                var givenNodes = definition.Descendants()
                    .Where(e => e.Name.LocalName == "GIVEN")
                    .Select(e => e.Value).ToList();
                var table = definition.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == "TABLE")?.Value;

                var forNodeObject = Nodes[forNode];
                forNodeObject.Parents = givenNodes;
                forNodeObject.ProbabilityTable = table;

                int expectedTableSize = forNodeObject.Outcomes.Count;
                foreach (var parent in givenNodes)
                {
                    expectedTableSize *= Nodes[parent].Outcomes.Count;
                }

                var parsedTable = table.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
                if (parsedTable.Length != expectedTableSize)
                {
                    throw new InvalidOperationException($"Invalid probability table size for node {forNode}. Expected {expectedTableSize}, found {parsedTable.Length}.");
                }
            }
        }


        private void SetEvidenceButton_Click(object sender, EventArgs e)
        {
            var evidenceForm = new EvidenceForm(Nodes);
            evidenceForm.ShowDialog();
        }

       
        private double GetProbability(Node node, string value, Dictionary<string, string> evidence)
        {
            // daca nodul nu are parinti, returneaza probabilitatea marginala
            if (node.Parents.Count == 0)
            {
                var probabilities = node.ProbabilityTable
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse)
                    .ToArray();

                int outcomeIndex = node.Outcomes.IndexOf(value);
                Console.WriteLine($"Node: {node.Name}, Value: {value}, Probability: {probabilities[outcomeIndex]}");
                return probabilities[outcomeIndex];
            }

            int index = 0;
            int multiplier = 1;

            for (int i = node.Parents.Count - 1; i >= 0; i--)
            {
                var parent = Nodes[node.Parents[i]];
                if (!evidence.ContainsKey(parent.Name))
                {
                    throw new InvalidOperationException($"Missing evidence for parent node {parent.Name}");
                }

                int parentIndex = parent.Outcomes.IndexOf(evidence[parent.Name]);
                index += parentIndex * multiplier;
                multiplier *= parent.Outcomes.Count;
            }

            index += node.Outcomes.IndexOf(value);

            var probabilitiesArray = node.ProbabilityTable
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(double.Parse)
                .ToArray();

            Console.WriteLine($"Node: {node.Name}, Value: {value}, Index: {index}, Probability: {probabilitiesArray[index]}");
            return probabilitiesArray[index];
        }

    }
}

