using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameCaro
{
    public enum KETTHUC
    {
        HoaCo,
        Player1,
        Player2,
        COM
    }
    class CaroChess
    {
        public static Pen pen;
        public static SolidBrush sbWhite;
        public static SolidBrush sbBlack;
        public static SolidBrush sbGreen;// used to undo
        private OCo[,] _MangOCo;
        private BanCo _BanCo;
        private int _LuotDi;
        private int _CheDoChoi;
        private bool _SanSang;
        private Stack<OCo> stkCacNuocDaDi;
        private Stack<OCo> stkCacNuocUndo;
        private KETTHUC _ketThuc;

        public bool SanSang
        {
            get { return _SanSang; }
        }
        public int CheDoChoi
        {
            get { return _CheDoChoi;}
        }
        public CaroChess()
        {
            pen = new Pen(Color.Green);// draw chess lines
            sbWhite = new SolidBrush(Color.White);
            sbBlack = new SolidBrush(Color.Black);
            sbGreen = new SolidBrush(Color.FromArgb(0, 192, 0));// match panel color 
            _BanCo = new BanCo(20, 20);
            _MangOCo = new OCo[_BanCo.SoDong, _BanCo.SoCot];
            stkCacNuocDaDi = new Stack<OCo>();
            stkCacNuocUndo = new Stack<OCo>();
            _LuotDi = 1;
        }

        public void VeBanCo(Graphics g)
        {
            _BanCo.VeBanCo(g);
        }

        public void KhoiTaoMangOCo()
        {
            for (int i = 0; i < _BanCo.SoDong; i++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++)
                {
                    _MangOCo[i, j] = new OCo(i, j, new Point(j * OCo._ChieuRong, i * OCo._ChieuCao), 0);
                }
            }
        }

        public bool DanhCo(int MouseX, int MouseY, Graphics g)
        {
            if (MouseX % OCo._ChieuRong == 0 || MouseY % OCo._ChieuCao == 0)// Click at border of chess piece
                return false;
            int Cot = MouseX / OCo._ChieuRong;
            int Dong = MouseY / OCo._ChieuCao;
            if (_MangOCo[Dong, Cot].SoHuu != 0)// can't click on clicked piece
                return false;

            switch (_LuotDi)// whose turn is this?
            {
                case 1:
                    _MangOCo[Dong, Cot].SoHuu = 1;
                    _BanCo.VeQuanCo(g, _MangOCo[Dong, Cot].ViTri, sbBlack);
                    _LuotDi = 2;
                    break;
                case 2:
                    _MangOCo[Dong, Cot].SoHuu = 2;
                    _BanCo.VeQuanCo(g, _MangOCo[Dong, Cot].ViTri, sbWhite);
                    _LuotDi = 1;
                    break;
                default:
                    MessageBox.Show("Có lỗi");
                    break;
            }
            stkCacNuocUndo = new Stack<OCo>();// delete old undo stack when play a new move
            OCo oco = new OCo(_MangOCo[Dong, Cot].Dong, _MangOCo[Dong, Cot].Cot, _MangOCo[Dong, Cot].ViTri, _MangOCo[Dong, Cot].SoHuu);
            stkCacNuocDaDi.Push(oco);// when undo, SoHuu becomes 0 => need to create new oco to prevent add oco with SoHuu = 0 to redo stack
            return true;
        }

        public void VeLaiQuanCo(Graphics g)// re-draw chess piece
        {
            foreach(OCo oco in stkCacNuocDaDi)
            {
                if (oco.SoHuu == 1)
                    _BanCo.VeQuanCo(g, oco.ViTri, sbBlack);
                else if (oco.SoHuu == 2)
                    _BanCo.VeQuanCo(g, oco.ViTri, sbWhite);
            }
        }

        public void StartPlayerVsPlayer(Graphics g)
        {
            _SanSang = true;
            stkCacNuocDaDi = new Stack<OCo>();
            stkCacNuocUndo = new Stack<OCo>();
            _LuotDi = 1;
            _CheDoChoi = 1;
            KhoiTaoMangOCo();
            VeBanCo(g);
        }

        public void StartPlayerVsCom(Graphics g)
        {
            _SanSang = true;
            stkCacNuocDaDi = new Stack<OCo>();
            stkCacNuocUndo = new Stack<OCo>();
            _LuotDi = 1;
            _CheDoChoi = 2;
            KhoiTaoMangOCo();
            VeBanCo(g);
            KhoiDongComputer(g);
        }

        #region Redo Undo
        public void Undo(Graphics g)
        {
            if (stkCacNuocDaDi.Count != 0)// undo when empty stack causes error
            {
                OCo oco = stkCacNuocDaDi.Pop();
                stkCacNuocUndo.Push(new OCo(oco.Dong, oco.Cot, oco.ViTri, oco.SoHuu));// for redo
                _MangOCo[oco.Dong, oco.Cot].SoHuu = 0;
                _BanCo.XoaQuanCo(g, oco.ViTri, sbGreen);
                if (_LuotDi == 1)// restore previous turn
                    _LuotDi = 2;
                else
                    _LuotDi = 1;
            }
            
        }

        public void Redo(Graphics g)
        {
            if (stkCacNuocUndo.Count != 0)// undo when empty stack causes error
            {
                OCo oco = stkCacNuocUndo.Pop();
                stkCacNuocDaDi.Push(new OCo(oco.Dong, oco.Cot, oco.ViTri, oco.SoHuu));// for undo
                _MangOCo[oco.Dong, oco.Cot].SoHuu = oco.SoHuu;
                _BanCo.VeQuanCo(g, oco.ViTri, oco.SoHuu == 1 ? sbBlack : sbWhite);
                if (_LuotDi == 1)
                    _LuotDi = 2;
                else
                    _LuotDi = 1;
            }

        }
        #endregion

        #region Duyệt chiến thắng
        public void KetThucTroChoi()
        {
            switch (_ketThuc)
            {
                case KETTHUC.HoaCo:
                    MessageBox.Show("Hòa cờ");
                    break;
                case KETTHUC.Player1:
                    MessageBox.Show("Người chơi 1 thắng");
                    break;
                case KETTHUC.Player2:
                    MessageBox.Show("Người chơi 2 thắng");
                    break;
                case KETTHUC.COM:
                    MessageBox.Show("Computer thắng");
                    break;
            }
            _SanSang = false;
        }
        public bool KiemTraChienThang()
        {
            if(stkCacNuocDaDi.Count == _BanCo.SoCot * _BanCo.SoDong)// out of move
            {
                _ketThuc = KETTHUC.HoaCo;
                return true;
            }

            foreach (OCo oco in stkCacNuocDaDi)
            {
                if(DuyetDoc(oco.Dong, oco.Cot, oco.SoHuu) || DuyetNgang(oco.Dong, oco.Cot, oco.SoHuu) || DuyetCheoXuoi(oco.Dong, oco.Cot, oco.SoHuu) || DuyetCheoNguoc(oco.Dong, oco.Cot, oco.SoHuu))
                {
                    _ketThuc = oco.SoHuu == 1 ? KETTHUC.Player1 : KETTHUC.Player2;
                    return true;
                }
            }
            return false;
        }

        private bool DuyetDoc(int currDong, int currCot, int currSoHuu)
        {
            if (currDong > _BanCo.SoDong - 5)// piece that can't make a 5 in a row, starting from it
                return false;
            int Dem;
            for(Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong + Dem, currCot].SoHuu != currSoHuu)// not 5 in 1 row
                    return false;
            }
            if (currDong == 0 || currDong + Dem == _BanCo.SoDong)// already 5 in a row, 1 piece starts (or ends) at the border
                return true;
            if (_MangOCo[currDong - 1, currCot].SoHuu == 0 || _MangOCo[currDong + Dem, currCot].SoHuu == 0)
                return true;
            return false;
        }
        private bool DuyetNgang(int currDong, int currCot, int currSoHuu)
        {
            if (currCot > _BanCo.SoCot - 5)// piece that can't make a 5 in a row, starting from it
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong, currCot + Dem].SoHuu != currSoHuu)// not 5 in 1 row
                    return false;
            }
            if (currCot == 0 || currCot + Dem == _BanCo.SoCot)// already 5 in a row, 1 piece starts (or ends) at the border
                return true;
            if (_MangOCo[currDong, currCot - 1].SoHuu == 0 || _MangOCo[currDong, currCot + Dem].SoHuu == 0)
                return true;
            return false;
        }
        private bool DuyetCheoXuoi(int currDong, int currCot, int currSoHuu)
        {
            if (currDong > _BanCo.SoDong - 5 || currCot > _BanCo.SoCot - 5)// piece that can't make a 5 in a row, starting from it
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu != currSoHuu)// not 5 in 1 row
                    return false;
            }
            if (currDong == 0 || currCot == 0 || currCot + Dem == _BanCo.SoCot || currDong + Dem == _BanCo.SoDong)// already 5 in a row, 1 piece starts (or ends) at the border
                return true;
            if (_MangOCo[currDong - 1, currCot - 1].SoHuu == 0 || _MangOCo[currDong + Dem, currCot + Dem].SoHuu == 0)
                return true;
            return false;
        }
        private bool DuyetCheoNguoc(int currDong, int currCot, int currSoHuu)
        {
            if (currDong < 4 || currCot > _BanCo.SoCot - 5)// piece that can't make a 5 in a row, starting from it
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu != currSoHuu)// not 5 in 1 row
                    return false;
            }
            if (currDong == _BanCo.SoDong - 1 || currCot == 0 || currDong == 4 || currCot + Dem == _BanCo.SoCot)// already 5 in a row, 1 piece starts (or ends) at the border
                return true;
            if (_MangOCo[currDong + 1, currCot - 1].SoHuu == 0 || _MangOCo[currDong - Dem, currCot + Dem].SoHuu == 0)
                return true;
            return false;
        }
        #endregion

        #region AI
        //private long[] MangDiemPhongNgu = new long[8] { 0, 9, 54, 162, 1458, 13112, 118008, 1062072 };
        private long[] MangDiemPhongNgu = new long[8] { 0, 9, 54, 108, 972, 8748, 78732, 708588 };// 8 means 4 in a row with 1 direction blocked happens twice
        //private long[] MangDiemTanCong = new long[8] { 0, 3, 27, 90, 738, 6561, 59049, 531036 };
        private long[] MangDiemTanCong = new long[8] { 0, 3, 27, 63, 495, 4374, 39366, 354294 };// 3 vs 9 means first move prioritize block, 63 (=54+9) and 495 (=486+9) means prioritize attack with 3 or 4 ally pieces, 27 to 54 (times 2 not 9) means 3 in a rows and 2 in 2 rows are both dangerous
        public void KhoiDongComputer(Graphics g)
        {
            if(stkCacNuocDaDi.Count == 0)
            {
                DanhCo(_BanCo.SoCot / 2 * OCo._ChieuRong + 1, _BanCo.SoDong / 2 * OCo._ChieuCao + 1, g);// +1 to avoid click on border
            }
            else
            {
                OCo oco = maxValue(_MangOCo, 5, -1000000, 1000000);
                DanhCo(oco.ViTri.X + 1, oco.ViTri.Y + 1, g);
            }
        }
        #region Định giá nước đi
        private long TinhDiem(int i, int j)
        {
            long DiemTanCong = DiemTanCong_DuyetDoc(i, j) + DiemTanCong_DuyetNgang(i, j) + DiemTanCong_DuyetCheoNguoc(i, j) + DiemTanCong_DuyetCheoXuoi(i, j);
            long DiemPhongNgu = DiemPhongNgu_DuyetDoc(i, j) + DiemPhongNgu_DuyetNgang(i, j) + DiemPhongNgu_DuyetCheoNguoc(i, j) + DiemPhongNgu_DuyetCheoXuoi(i, j);
            long DiemTam = DiemTanCong > DiemPhongNgu ? DiemTanCong : DiemPhongNgu;
            return DiemTam;
        }
        private long TinhDiemPlayer(int i, int j)
        {
            long DiemTanCong = DiemTanCong_DuyetDocPlayer(i, j) + DiemTanCong_DuyetNgangPlayer(i, j) + DiemTanCong_DuyetCheoNguocPlayer(i, j) + DiemTanCong_DuyetCheoXuoiPlayer(i, j);
            long DiemPhongNgu = DiemPhongNgu_DuyetDocPlayer(i, j) + DiemPhongNgu_DuyetNgangPlayer(i, j) + DiemPhongNgu_DuyetCheoNguocPlayer(i, j) + DiemPhongNgu_DuyetCheoXuoiPlayer(i, j);
            long DiemTam = DiemTanCong > DiemPhongNgu ? DiemTanCong : DiemPhongNgu;
            return DiemTam;
        }
        private OCo TimKiemNuocDi()
        {
            OCo oCoResult = new OCo();
            long DiemMax = 0;
            for (int i = 0; i < _BanCo.SoDong; i++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++){
                    if (_MangOCo[i, j].SoHuu == 0)
                    {
                        long DiemTanCong = DiemTanCong_DuyetDoc(i, j) + DiemTanCong_DuyetNgang(i, j) + DiemTanCong_DuyetCheoNguoc(i, j) + DiemTanCong_DuyetCheoXuoi(i, j);
                        long DiemPhongNgu = DiemPhongNgu_DuyetDoc(i, j) + DiemPhongNgu_DuyetNgang(i, j) + DiemPhongNgu_DuyetCheoNguoc(i, j) + DiemPhongNgu_DuyetCheoXuoi(i, j);
                        long DiemTam = DiemTanCong > DiemPhongNgu ? DiemTanCong : DiemPhongNgu;
                        if (DiemMax < DiemTam)
                        {
                            DiemMax = DiemTam;
                            oCoResult = new OCo(_MangOCo[i, j].Dong, _MangOCo[i, j].Cot, _MangOCo[i, j].ViTri, _MangOCo[i, j].SoHuu);
                        }
                    }
                }
            }

            return oCoResult;
        }
        #endregion
        #region Alpha-Beta Pruning Algorithm
        private OCo maxValue(OCo[,] _MangOCo, long depth, long alpha, long beta)
        {
            if (depth == 0)
            {
                OCo oCoMax = new OCo();
                oCoMax = TimKiemNuocDi();
                return oCoMax;
            }
            long v = -1000000;
            OCo oCoResult = new OCo();
            for (int i = 0; i < _BanCo.SoDong; i++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++)
                {
                    if (_MangOCo[i, j].SoHuu == 0)
                    {
                        long currScore = TinhDiem(i, j);
                        _MangOCo[i, j].SoHuu = 1;
                        OCo oCoMin = new OCo();
                        oCoMin = minValue(_MangOCo, depth - 1, alpha, beta);
                        if(currScore - TinhDiemPlayer(oCoMin.Dong, oCoMin.Cot) > v)
                        {
                            oCoResult = new OCo(_MangOCo[i, j].Dong, _MangOCo[i, j].Cot, _MangOCo[i, j].ViTri, _MangOCo[i, j].SoHuu);
                            v = currScore - TinhDiemPlayer(oCoMin.Dong, oCoMin.Cot);
                        }
                        if (v >= beta)
                        {
                            _MangOCo[i, j].SoHuu = 0;
                            return oCoResult;
                        }
                        alpha = alpha > v ? alpha : v;
                        _MangOCo[i, j].SoHuu = 0;
                    }
                }
            }
            return oCoResult;
        }
        private OCo minValue(OCo[,] _MangOCo, long depth, long alpha, long beta)
        {
            if (depth == 0)
            {
                OCo oCoMax = new OCo();
                oCoMax = TimKiemNuocDi();
                return oCoMax;
            }
            long v = -1000000;
            OCo oCoResult = new OCo();
            for (int i = 0; i < _BanCo.SoDong; i++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++)
                {
                    if (_MangOCo[i, j].SoHuu == 0)
                    {
                        long currScore = TinhDiemPlayer(i, j);
                        _MangOCo[i, j].SoHuu = 1;
                        OCo oCoMin = new OCo();
                        oCoMin = minValue(_MangOCo, depth - 1, alpha, beta);
                        if (TinhDiem(oCoMin.Dong, oCoMin.Cot) - currScore < v)
                        {
                            oCoResult = new OCo(_MangOCo[i, j].Dong, _MangOCo[i, j].Cot, _MangOCo[i, j].ViTri, _MangOCo[i, j].SoHuu);
                            v = TinhDiem(oCoMin.Dong, oCoMin.Cot) - currScore;
                        }
                        if (v <= alpha)
                        {
                            _MangOCo[i, j].SoHuu = 0;
                            return oCoResult;
                        }
                        beta = beta < v ? beta : v;
                        _MangOCo[i, j].SoHuu = 0;
                    }
                }
            }
            return oCoResult;
        }
        #endregion
        #region Tấn công MAX
        private long DiemTanCong_DuyetDoc(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currDong + Dem < _BanCo.SoDong; Dem++)// bottom up check
            {
                if (_MangOCo[currDong + Dem, currCot].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong + Dem, currCot].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currDong - Dem >= 0; Dem++)// top down check
            {
                if (_MangOCo[currDong - Dem, currCot].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong - Dem, currCot].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa]*2;       //
            
            return DiemTong;
        }
        private long DiemTanCong_DuyetNgang(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot; Dem++)// left right check
            {
                if (_MangOCo[currDong, currCot + Dem].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong, currCot + Dem].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0; Dem++)// right left check
            {
                if (_MangOCo[currDong, currCot - Dem].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong, currCot - Dem].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa]*2;       //

            return DiemTong;
        }
        private long DiemTanCong_DuyetCheoXuoi(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong + Dem < _BanCo.SoDong; Dem++)// top-left down check
            {
                if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong - Dem >= 0; Dem++)// bottom-right up check
            {
                if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa]*2;       //

            return DiemTong;
        }
        private long DiemTanCong_DuyetCheoNguoc(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong - Dem >= 0; Dem++)// left-bottom up check
            {
                if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong + Dem < _BanCo.SoDong; Dem++)// top-right down check
            {
                if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 1)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa]*2;       //

            return DiemTong;
        }
        #endregion
        #region Phòng ngự MAX
        private long DiemPhongNgu_DuyetDoc(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currDong + Dem < _BanCo.SoDong; Dem++)// bottom up check
            {
                if (_MangOCo[currDong + Dem, currCot].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;
                    break;
                }
                else if (_MangOCo[currDong + Dem, currCot].SoHuu == 2)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                }
                else
                {
                    if (Dem == 1 && currDong + Dem + 3 < _BanCo.SoDong)
                    {
                        if (_MangOCo[currDong + Dem + 1, currCot].SoHuu == 2 && _MangOCo[currDong + Dem + 2, currCot].SoHuu == 2)
                        {
                            switch(_MangOCo[currDong + Dem + 3, currCot].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong + Dem + 2 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot].SoHuu == 2 && _MangOCo[currDong + Dem + 2, currCot].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong + Dem + 1 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currDong - Dem >= 0; Dem++)// top down check
            {
                if (_MangOCo[currDong - Dem, currCot].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong - Dem, currCot].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currDong - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong - Dem - 1, currCot].SoHuu == 2 && _MangOCo[currDong - Dem - 2, currCot].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong - Dem - 3, currCot].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong - Dem - 2 >= 0 && _MangOCo[currDong - Dem - 1, currCot].SoHuu == 2 && _MangOCo[currDong - Dem - 2, currCot].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong - Dem - 1 >= 0 && _MangOCo[currDong - Dem - 1, currCot].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic

            return DiemTong;
        }
        private long DiemPhongNgu_DuyetNgang(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot; Dem++)// left right check
            {
                if (_MangOCo[currDong, currCot + Dem].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong, currCot + Dem].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot + Dem + 3 < _BanCo.SoCot)
                    {
                        if (_MangOCo[currDong, currCot + Dem + 1].SoHuu == 2 && _MangOCo[currDong, currCot + Dem + 2].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong, currCot + Dem + 3].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot + Dem + 2 < _BanCo.SoCot && _MangOCo[currDong, currCot + Dem + 1].SoHuu == 2 && _MangOCo[currDong, currCot + Dem + 2].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot + Dem + 1 < _BanCo.SoCot && _MangOCo[currDong, currCot + Dem + 1].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0; Dem++)// right left check
            {
                if (_MangOCo[currDong, currCot - Dem].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong, currCot - Dem].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong, currCot - Dem - 1].SoHuu == 2 && _MangOCo[currDong, currCot - Dem - 2].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong, currCot - Dem - 3].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot - Dem - 2 >= 0 && _MangOCo[currDong, currCot - Dem - 1].SoHuu == 2 && _MangOCo[currDong, currCot - Dem - 2].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot - Dem - 1 >= 0 && _MangOCo[currDong, currCot - Dem - 1].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic    

            return DiemTong;
        }
        private long DiemPhongNgu_DuyetCheoXuoi(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong + Dem < _BanCo.SoDong; Dem++)// top-left down check
            {
                if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot + Dem + 3 < _BanCo.SoCot && currDong + Dem + 3 < _BanCo.SoDong)
                    {
                        if (_MangOCo[currDong + Dem + 1, currCot + Dem + 1].SoHuu == 2 && _MangOCo[currDong + Dem + 2, currCot + Dem + 2].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong + Dem + 3, currCot + Dem + 3].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot + Dem + 2 < _BanCo.SoCot && currDong + Dem + 2 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot + Dem + 1].SoHuu == 2 && _MangOCo[currDong + Dem + 2, currCot + Dem + 2].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot + Dem + 1 < _BanCo.SoCot && currDong + Dem + 1 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot + Dem + 1].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong - Dem >= 0; Dem++)// bottom-right up check
            {
                if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot - Dem - 3 >= 0 && currDong - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong - Dem - 1, currCot - Dem - 1].SoHuu == 2 && _MangOCo[currDong - Dem - 2, currCot - Dem - 2].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong - Dem - 3, currCot - Dem - 3].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot - Dem - 2 >= 0 && currDong - Dem - 2 >= 0 && _MangOCo[currDong - Dem - 1, currCot - Dem - 1].SoHuu == 2 && _MangOCo[currDong - Dem -2, currCot - Dem - 2].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot - Dem - 1 >= 0 && currDong - Dem - 1 >= 0 && _MangOCo[currDong - Dem - 1, currCot - Dem - 1].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa== 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic

            return DiemTong;
        }
        private long DiemPhongNgu_DuyetCheoNguoc(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong - Dem >= 0; Dem++)// left-bottom up check
            {
                if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currDong - Dem - 3 >= 0 && currCot + Dem + 3 < _BanCo.SoCot)
                    {
                        if (_MangOCo[currDong - Dem - 1, currCot + Dem + 1].SoHuu == 2 && _MangOCo[currDong - Dem - 2, currCot + Dem + 2].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong - Dem - 3, currCot + Dem + 3].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong - Dem - 2 >= 0 && currCot + Dem + 2 < _BanCo.SoCot && _MangOCo[currDong - Dem - 1, currCot + Dem + 1].SoHuu == 2 && _MangOCo[currDong - Dem - 2, currCot + Dem + 2].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong - Dem - 1 >= 0 && currCot + Dem + 1 < _BanCo.SoCot && _MangOCo[currDong - Dem - 1, currCot + Dem + 1].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong + Dem < _BanCo.SoDong; Dem++)// top-right down check
            {
                if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 1)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 2)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currDong + Dem + 3 < _BanCo.SoDong && currCot - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong + Dem + 1, currCot - Dem - 1].SoHuu == 2 && _MangOCo[currDong + Dem + 2, currCot - Dem - 2].SoHuu == 2)
                        {
                            switch (_MangOCo[currDong + Dem + 3, currCot - Dem - 3].SoHuu)
                            {
                                case 1: break;
                                case 2: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong + Dem + 2 < _BanCo.SoDong && currCot - Dem - 2 >= 0 && _MangOCo[currDong + Dem + 1, currCot - Dem - 1].SoHuu == 2 && _MangOCo[currDong + Dem + 2, currCot - Dem - 2].SoHuu == 2)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong + Dem + 1 < _BanCo.SoDong && currCot - Dem - 1 >= 0 && _MangOCo[currDong + Dem + 1, currCot - Dem - 1].SoHuu == 2)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic

            return DiemTong;
        }
        #endregion
        #region Tấn công MIN
        private long DiemTanCong_DuyetDocPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currDong + Dem < _BanCo.SoDong; Dem++)// bottom up check
            {
                if (_MangOCo[currDong + Dem, currCot].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong + Dem, currCot].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currDong - Dem >= 0; Dem++)// top down check
            {
                if (_MangOCo[currDong - Dem, currCot].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong - Dem, currCot].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa] * 2;       //

            return DiemTong;
        }
        private long DiemTanCong_DuyetNgangPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot; Dem++)// left right check
            {
                if (_MangOCo[currDong, currCot + Dem].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong, currCot + Dem].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0; Dem++)// right left check
            {
                if (_MangOCo[currDong, currCot - Dem].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong, currCot - Dem].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa] * 2;       //

            return DiemTong;
        }
        private long DiemTanCong_DuyetCheoXuoiPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong + Dem < _BanCo.SoDong; Dem++)// top-left down check
            {
                if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong - Dem >= 0; Dem++)// bottom-right up check
            {
                if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa] * 2;       //

            return DiemTong;
        }
        private long DiemTanCong_DuyetCheoNguocPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong - Dem >= 0; Dem++)// left-bottom up check
            {
                if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong + Dem < _BanCo.SoDong; Dem++)// top-right down check
            {
                if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 2)// by default, computer go first
                    SoQuanTa++;
                else if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                    break;
                }
                else
                {
                    break;// can improve
                }
            }
            if (SoQuanDich == 2)// blocked at both directions
                return 0;
            DiemTong -= MangDiemPhongNgu[SoQuanDich];// heuristic
            DiemTong += MangDiemTanCong[SoQuanTa] * 2;       //

            return DiemTong;
        }
        #endregion
        #region Phòng ngự MIN
        private long DiemPhongNgu_DuyetDocPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currDong + Dem < _BanCo.SoDong; Dem++)// bottom up check
            {
                if (_MangOCo[currDong + Dem, currCot].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;
                    break;
                }
                else if (_MangOCo[currDong + Dem, currCot].SoHuu == 1)
                {
                    SoQuanDich++;// meet enemy piece then abandon that move
                }
                else
                {
                    if (Dem == 1 && currDong + Dem + 3 < _BanCo.SoDong)
                    {
                        if (_MangOCo[currDong + Dem + 1, currCot].SoHuu == 1 && _MangOCo[currDong + Dem + 2, currCot].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong + Dem + 3, currCot].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong + Dem + 2 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot].SoHuu == 1 && _MangOCo[currDong + Dem + 2, currCot].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong + Dem + 1 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currDong - Dem >= 0; Dem++)// top down check
            {
                if (_MangOCo[currDong - Dem, currCot].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong - Dem, currCot].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currDong - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong - Dem - 1, currCot].SoHuu == 1 && _MangOCo[currDong - Dem - 2, currCot].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong - Dem - 3, currCot].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong - Dem - 2 >= 0 && _MangOCo[currDong - Dem - 1, currCot].SoHuu == 1 && _MangOCo[currDong - Dem - 2, currCot].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong - Dem - 1 >= 0 && _MangOCo[currDong - Dem - 1, currCot].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic

            return DiemTong;
        }
        private long DiemPhongNgu_DuyetNgangPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot; Dem++)// left right check
            {
                if (_MangOCo[currDong, currCot + Dem].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong, currCot + Dem].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot + Dem + 3 < _BanCo.SoCot)
                    {
                        if (_MangOCo[currDong, currCot + Dem + 1].SoHuu == 1 && _MangOCo[currDong, currCot + Dem + 2].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong, currCot + Dem + 3].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot + Dem + 2 < _BanCo.SoCot && _MangOCo[currDong, currCot + Dem + 1].SoHuu == 1 && _MangOCo[currDong, currCot + Dem + 2].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot + Dem + 1 < _BanCo.SoCot && _MangOCo[currDong, currCot + Dem + 1].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0; Dem++)// right left check
            {
                if (_MangOCo[currDong, currCot - Dem].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong, currCot - Dem].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong, currCot - Dem - 1].SoHuu == 1 && _MangOCo[currDong, currCot - Dem - 2].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong, currCot - Dem - 3].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot - Dem - 2 >= 0 && _MangOCo[currDong, currCot - Dem - 1].SoHuu == 1 && _MangOCo[currDong, currCot - Dem - 2].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot - Dem - 1 >= 0 && _MangOCo[currDong, currCot - Dem - 1].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic    

            return DiemTong;
        }
        private long DiemPhongNgu_DuyetCheoXuoiPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong + Dem < _BanCo.SoDong; Dem++)// top-left down check
            {
                if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot + Dem + 3 < _BanCo.SoCot && currDong + Dem + 3 < _BanCo.SoDong)
                    {
                        if (_MangOCo[currDong + Dem + 1, currCot + Dem + 1].SoHuu == 1 && _MangOCo[currDong + Dem + 2, currCot + Dem + 2].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong + Dem + 3, currCot + Dem + 3].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot + Dem + 2 < _BanCo.SoCot && currDong + Dem + 2 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot + Dem + 1].SoHuu == 1 && _MangOCo[currDong + Dem + 2, currCot + Dem + 2].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot + Dem + 1 < _BanCo.SoCot && currDong + Dem + 1 < _BanCo.SoDong && _MangOCo[currDong + Dem + 1, currCot + Dem + 1].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong - Dem >= 0; Dem++)// bottom-right up check
            {
                if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong - Dem, currCot - Dem].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currCot - Dem - 3 >= 0 && currDong - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong - Dem - 1, currCot - Dem - 1].SoHuu == 1 && _MangOCo[currDong - Dem - 2, currCot - Dem - 2].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong - Dem - 3, currCot - Dem - 3].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currCot - Dem - 2 >= 0 && currDong - Dem - 2 >= 0 && _MangOCo[currDong - Dem - 1, currCot - Dem - 1].SoHuu == 1 && _MangOCo[currDong - Dem - 2, currCot - Dem - 2].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currCot - Dem - 1 >= 0 && currDong - Dem - 1 >= 0 && _MangOCo[currDong - Dem - 1, currCot - Dem - 1].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic

            return DiemTong;
        }
        private long DiemPhongNgu_DuyetCheoNguocPlayer(int currDong, int currCot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            for (int Dem = 1; Dem < 6 && currCot + Dem < _BanCo.SoCot && currDong - Dem >= 0; Dem++)// left-bottom up check
            {
                if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currDong - Dem - 3 >= 0 && currCot + Dem + 3 < _BanCo.SoCot)
                    {
                        if (_MangOCo[currDong - Dem - 1, currCot + Dem + 1].SoHuu == 1 && _MangOCo[currDong - Dem - 2, currCot + Dem + 2].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong - Dem - 3, currCot + Dem + 3].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong - Dem - 2 >= 0 && currCot + Dem + 2 < _BanCo.SoCot && _MangOCo[currDong - Dem - 1, currCot + Dem + 1].SoHuu == 1 && _MangOCo[currDong - Dem - 2, currCot + Dem + 2].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong - Dem - 1 >= 0 && currCot + Dem + 1 < _BanCo.SoCot && _MangOCo[currDong - Dem - 1, currCot + Dem + 1].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }

            for (int Dem = 1; Dem < 6 && currCot - Dem >= 0 && currDong + Dem < _BanCo.SoDong; Dem++)// top-right down check
            {
                if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 2)// by default, computer go first
                {
                    SoQuanTa++;// meet ally piece then abandon that move
                    break;
                }
                else if (_MangOCo[currDong + Dem, currCot - Dem].SoHuu == 1)
                {
                    SoQuanDich++;
                }
                else
                {
                    if (Dem == 1 && currDong + Dem + 3 < _BanCo.SoDong && currCot - Dem - 3 >= 0)
                    {
                        if (_MangOCo[currDong + Dem + 1, currCot - Dem - 1].SoHuu == 1 && _MangOCo[currDong + Dem + 2, currCot - Dem - 2].SoHuu == 1)
                        {
                            switch (_MangOCo[currDong + Dem + 3, currCot - Dem - 3].SoHuu)
                            {
                                case 2: break;
                                case 1: SoQuanDich += 2; break;
                                default: SoQuanDich++; break;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (Dem == 2 && currDong + Dem + 2 < _BanCo.SoDong && currCot - Dem - 2 >= 0 && _MangOCo[currDong + Dem + 1, currCot - Dem - 1].SoHuu == 1 && _MangOCo[currDong + Dem + 2, currCot - Dem - 2].SoHuu == 1)// 3/4 in a rows without any block
                    {
                        SoQuanDich += 2;
                        break;
                    }
                    else if (Dem == 3 && currDong + Dem + 1 < _BanCo.SoDong && currCot - Dem - 1 >= 0 && _MangOCo[currDong + Dem + 1, currCot - Dem - 1].SoHuu == 1)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        break;// can improve
                    }
                }
            }
            if (SoQuanTa == 2)// blocked at both directions
                return 0;
            if (SoQuanTa == 1 && SoQuanDich == 3)
                return MangDiemPhongNgu[2];
            DiemTong -= MangDiemPhongNgu[SoQuanTa];
            DiemTong += MangDiemPhongNgu[SoQuanDich];// heuristic

            return DiemTong;
        }
        #endregion
        #endregion
    }
}
