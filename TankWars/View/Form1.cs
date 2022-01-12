using System;
using System.Drawing;
using System.Windows.Forms;
using TankWars;

/*
 * Dune (Tank) Wars
 * By: Matthew Streicher and Jim Morgan
 * University of Utah
 * CS3500 - Software Practice 1
 * Fall 2021
 */

namespace View
{
    public partial class Form1 : Form
    {
        //World theWorld;
        GameController gameController;
        DrawingPanel panel;

        private const int viewSize = 900;
        private const int menuSize = 40;

        public Form1()
        {
            InitializeComponent();
            gameController = new GameController(); 
            ClientSize = new Size(viewSize, viewSize + menuSize);

            // Set up key and mouse handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;

            // Handle gamecontroller events
            gameController.ErrorOccurred += ErrorMessagePopup;
            gameController.ReceivedSetupInfo += SetupDrawingPanel;
            gameController.UpdateArrived += OnFrame;
        }

        private void ConnectButton_Click(object sender, System.EventArgs e)
        {
            string player = NameTextBox.Text;
            string IpAddress = AddressBox.Text;

            if (IpAddress.Equals("") || player.Equals(""))
            { 
                MessageBox.Show("IP Address and Player name required");
            }
            else
                gameController.Connect(IpAddress, player);

        }

        private void ErrorMessagePopup(string errorMessage)
        {
            MessageBox.Show(errorMessage);
        }

        private void SetupDrawingPanel()
        {
            this.Invoke(new MethodInvoker(DrawingPanelSetup));
        }

        private void DrawingPanelSetup()
        {
            ConnectButton.Enabled = false;
            NameTextBox.Enabled = false;

            // Enable the global form to capture key presses
            KeyPreview = true;

            // Place and add the drawing panel
            panel = new DrawingPanel(gameController)
            {
                Location = new Point(0, menuSize),
                Size = new Size(viewSize, viewSize)
            };
            this.Controls.Add(panel);

            panel.MouseDown += HandleMouseDown;
            panel.MouseUp += HandleMouseUp;
            panel.MouseMove += HandleMouseMoved;
        }

        // **********Keyboard Methods**********

        /// <summary>
        /// Key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            string direction = "none";

            switch (e.KeyCode)
            {
                case Keys.W:
                    direction = "up";
                    break;
                case Keys.A:
                    direction = "left";
                    break;
                case Keys.S:
                    direction = "down";
                    break;
                case Keys.D:
                    direction = "right";
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
                default:
                    break;
            }
            gameController.HandleMoveRequest(direction);

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Key up handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    gameController.CancelMoveRequest();
                    break;
                case Keys.A:
                    gameController.CancelMoveRequest();
                    break;
                case Keys.S:
                    gameController.CancelMoveRequest();
                    break;
                case Keys.D:
                    gameController.CancelMoveRequest();
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
            }
        }

        private void HandleMouseMoved(object sender, MouseEventArgs e)
        {
            Invalidate(true);
            Vector2D cursorLoc = new Vector2D(e.X - (viewSize / 2), e.Y - (viewSize / 2));
            cursorLoc.Normalize();
            gameController.MouseMove(cursorLoc);
        }

        /// <summary>
        /// Handle mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            string fire = "";

            if (e.Button == MouseButtons.Left)
                fire = "main";
            if (e.Button == MouseButtons.Right)
                fire = "alt";
            gameController.HandleMouseRequest(fire);
        }

        /// <summary>
        /// Handle mouse up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                gameController.CancelMouseRequest();
            if (e.Button == MouseButtons.Right)
                gameController.CancelMouseRequest();
        }

        private void OnFrame()
        {
            try
            {
                this.Invoke(new MethodInvoker(() => Invalidate(true)));
            }
            catch (System.Exception)
            {
            }
        }
        //Displays control menu in help toolbar
        private void controlsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("Keyboard Controls" + Environment.NewLine + 
                            "W: Move up" + Environment.NewLine +
                            "A: Move left" + Environment.NewLine +
                            "S: Move Down" + Environment.NewLine +
                            "D: Move right" + Environment.NewLine +
                            Environment.NewLine +
                            "Mouse Contols" + Environment.NewLine +
                            "Left click: Fire projectile" + Environment.NewLine +
                            "Right click: Fire Beam" + Environment.NewLine +
                            Environment.NewLine +
                            "Escape: Quit Game");
        }

        private void AddressBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
