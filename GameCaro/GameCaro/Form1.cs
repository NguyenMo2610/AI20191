using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameCaro
{
    public partial class frmCoCaro : Form
    {
        private CaroChess caroChess;
        private Graphics grs;
        public frmCoCaro()
        {
            InitializeComponent();
            btnPlayervsPlayer.Click += new EventHandler(PvsP);
            caroChess = new CaroChess();
            caroChess.KhoiTaoMangOCo();
            grs = pnlBanCo.CreateGraphics();

            playerVsComputerToolStripMenuItem.Click += new EventHandler(PvsC_Click);
            btnPlayervsCom.Click += new EventHandler(PvsC_Click);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tmChuChay_Tick(object sender, EventArgs e)
        {
            lblChuoiChu.Location = new Point(lblChuoiChu.Location.X, lblChuoiChu.Location.Y - 1);
            if (lblChuoiChu.Location.Y + lblChuoiChu.Height < 0)
            {
                lblChuoiChu.Location = new Point(lblChuoiChu.Location.X, pnlChuChay.Height);
            }
        }

        private void frmCoCaro_Load(object sender, EventArgs e)
        {
            lblChuoiChu.Text = "Luật chơi: 5 quân cùng 1 hàng,\n1 cột hoặc 1 đường chéo không\nbị chặn 2 đầu để giành chiến thắng";
            tmChuChay.Enabled = true;
            caroChess.VeBanCo(grs);
        }

        private void pnlBanCo_Paint(object sender, PaintEventArgs e)
        {
            caroChess.VeBanCo(grs);
            caroChess.VeLaiQuanCo(grs);
        }

        private void pnlBanCo_MouseClick(object sender, MouseEventArgs e)
        {
            if (!caroChess.SanSang)// pick mode before play
                return;
            if (caroChess.DanhCo(e.X, e.Y, grs))
            {
                if (caroChess.KiemTraChienThang())
                    caroChess.KetThucTroChoi();
                else
                {
                    if (caroChess.CheDoChoi == 2)
                    {
                        caroChess.KhoiDongComputer(grs);
                        if (caroChess.KiemTraChienThang())
                            caroChess.KetThucTroChoi();
                    }
                }
            }
        }

        private void PvsP(object sender, EventArgs e)
        {
            grs.Clear(pnlBanCo.BackColor);
            caroChess.StartPlayerVsPlayer(grs);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (caroChess.CheDoChoi == 1)
            {
                caroChess.Undo(grs);
            }
            else
            {
                caroChess.Undo(grs);
                caroChess.Undo(grs);
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (caroChess.CheDoChoi == 1)
            {
                caroChess.Redo(grs);
            }
            else
            {
                caroChess.Redo(grs);
                caroChess.Redo(grs);
            }
        }
        private void PvsC_Click(object sender, EventArgs e)
        {
            grs.Clear(pnlBanCo.BackColor);
            caroChess.StartPlayerVsCom(grs);
        }
    }
}
