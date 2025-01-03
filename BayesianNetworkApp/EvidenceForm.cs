using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayesianNetworkApp
{
    public class EvidenceForm : Form
    {
        private Dictionary<string, Node> Nodes;

        public EvidenceForm(Dictionary<string, Node> nodes)
        {
            Nodes = nodes;

            this.Text = "Set Evidence";
            this.Width = 400;
            this.Height = 300;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };

            foreach (var node in Nodes.Values)
            {
                var label = new Label { Text = node.Name, Dock = DockStyle.Fill };
                var comboBox = new ComboBox { Dock = DockStyle.Fill };

                comboBox.Items.AddRange(node.Outcomes.ToArray());
                comboBox.SelectedIndexChanged += (s, e) =>
                {
                    node.Evidence = comboBox.SelectedItem?.ToString();
                };

                layout.Controls.Add(label);
                layout.Controls.Add(comboBox);
            }

            this.Controls.Add(layout);
        }
    }
}
