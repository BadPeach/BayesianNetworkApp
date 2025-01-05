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
            queryButton.Click += QueryButton_Click;
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

        private void QueryButton_Click(object sender, EventArgs e)
        {
            string selectedNode = queryNodeComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedNode))
            {
                MessageBox.Show("Please select a node to query.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double probability = Infer(selectedNode);

            MessageBox.Show($"Probabilitatea pentru {selectedNode} = Da: {probability:F5}", "Query Results", MessageBoxButtons.OK, MessageBoxIcon.Information);

            graphPanel.Refresh();

            using (Graphics g = graphPanel.CreateGraphics())
            {
                g.Clear(Color.White);
                if (Nodes == null) return;

                // pasul 1: calcularea nivelelor nodurilor
                var nodeLevels = new Dictionary<string, int>();
                var visited = new HashSet<string>();

                foreach (var node in Nodes.Values)
                {
                    // daca nodul nu are parinti, calc. nivelele
                    if (node.Parents.Count == 0)
                    {
                        CalculateNodeLevels(node.Name, 0, nodeLevels, visited);
                    }
                }

                // pasul 2: gruparea nodurilor in functie de nivel
                var levels = nodeLevels.GroupBy(kvp => kvp.Value)
                                       .OrderBy(g => g.Key)
                                       .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key).ToList());

                var nodeRadius = 70;
                var spacingX = 150;
                var spacingY = 120;
                var panelWidth = graphPanel.Width;

                var nodePositions = new Dictionary<string, Point>();

                foreach (var level in levels)
                {
                    int y = 50 + level.Key * spacingY;
                    int count = level.Value.Count;
                    int totalWidth = (count - 1) * spacingX;
                    int startX = (panelWidth - totalWidth) / 2;

                    for (int i = 0; i < count; i++)
                    {
                        string nodeName = level.Value[i];
                        int x = startX + i * spacingX;

                        nodePositions[nodeName] = new Point(x, y);

                        g.FillEllipse(Brushes.LightBlue, x - nodeRadius / 2, y - nodeRadius / 2, nodeRadius, nodeRadius);
                        g.DrawEllipse(Pens.Black, x - nodeRadius / 2, y - nodeRadius / 2, nodeRadius, nodeRadius);

                        g.DrawString(nodeName, new Font("Times New Roman", 10), Brushes.Black, x - 20, y - 10);
                    }
                }

                // pasul 4: desenarea conexiunilor
                foreach (var node in Nodes.Values)
                {
                    foreach (var parent in node.Parents)
                    {
                        if (nodePositions.ContainsKey(node.Name) && nodePositions.ContainsKey(parent))
                        {
                            var childPos = nodePositions[node.Name];
                            var parentPos = nodePositions[parent];
                            var start = new Point(parentPos.X, parentPos.Y + nodeRadius / 2);
                            var end = new Point(childPos.X, childPos.Y - nodeRadius / 2);

                            g.DrawLine(Pens.Black, start, end);
                            DrawArrow(g, start, end);
                        }
                    }
                }
            }
        }

        private void DrawArrow(Graphics g, Point start, Point end)
        {
            var arrowSize = 10;
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            // calculeaza coordonatele varfului sagetii
            var arrowPoint1 = new Point(
                (int)(end.X - arrowSize * Math.Cos(angle - Math.PI / 6)),
                (int)(end.Y - arrowSize * Math.Sin(angle - Math.PI / 6))
            );
            var arrowPoint2 = new Point(
                (int)(end.X - arrowSize * Math.Cos(angle + Math.PI / 6)),
                (int)(end.Y - arrowSize * Math.Sin(angle + Math.PI / 6))
            );

            g.DrawLine(Pens.Black, end, arrowPoint1);
            g.DrawLine(Pens.Black, end, arrowPoint2);
        }

        private void CalculateNodeLevels(string nodeName, int level, Dictionary<string, int> nodeLevels, HashSet<string> visited)
        {
            if (visited.Contains(nodeName)) return;

            visited.Add(nodeName);
            nodeLevels[nodeName] = level;

            var node = Nodes[nodeName];
            foreach (var child in Nodes.Values.Where(n => n.Parents.Contains(nodeName)))
            {
                CalculateNodeLevels(child.Name, level + 1, nodeLevels, visited);
            }
        }

        private double Infer(string targetNode)
        {
            var queryNode = Nodes[targetNode];

            // calculeaza probabilitatile pentru fiecare valoare posibila a nodului interogat
            Dictionary<string, double> probabilities = new();

            foreach (var outcome in queryNode.Outcomes)
            {
                probabilities[outcome] = EnumerateAll(Nodes.Keys.ToList(), new Dictionary<string, string>
                {
                    { targetNode, outcome }
                });
            }

            // normalizarea rezultatelor
            double total = probabilities.Values.Sum();
            foreach (var key in probabilities.Keys.ToList())
            {
                probabilities[key] /= total;
            }
            Console.WriteLine($"Infer - Total before normalization: {total}");

            return probabilities["Da"];
        }

        private double EnumerateAll(List<string> variables, Dictionary<string, string> evidence)
        {
            if (!variables.Any())
            {
                return 1.0;
            }

            var first = variables.First();
            var rest = variables.Skip(1).ToList();

            var currentNode = Nodes[first];

            // daca variabila are o evidenta, calculeaza probabilitatea conditionata direct
            if (evidence.ContainsKey(first))
            {
                double prob = GetProbability(currentNode, evidence[first], evidence);
                Console.WriteLine($"EnumerateAll - Node: {first}, Evidence: {evidence[first]}, Probability: {prob}");
                return prob * EnumerateAll(rest, evidence);
            }

            // daca variabila nu are evidenta, sumeaza probabilitatile pentru toate valorile posibile
            double total = 0.0;

            foreach (var outcome in currentNode.Outcomes)
            {
                var extendedEvidence = new Dictionary<string, string>(evidence)
                {
                     { first, outcome }
                };

                double prob = GetProbability(currentNode, outcome, evidence);
                double subtotal = prob * EnumerateAll(rest, extendedEvidence);
                Console.WriteLine($"EnumerateAll - Node: {first}, Outcome: {outcome}, Subtotal: {subtotal}");
                total += subtotal;
            }

            return total;
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

        private void aboutClick(object sender, EventArgs e)
        {
            MessageBox.Show("This application was built for managing Bayesian Networks.\nVersion 1.0", "About");
        }

        private void quitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

