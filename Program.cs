using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DatAnalyzer
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        // UI 컨트롤 선언
        private Button btnOpenFile;
        private TextBox txtFilePath;
        private TabControl tabControl;
        private TabPage tabBinary;
        private TabPage tabEncoding;
        
        // 바이너리 탭 컨트롤
        private TextBox txtBinaryResult;
        private ComboBox cmbByteGroup;
        private Button btnAnalyzeBinary;

        // 인코딩 탭 컨트롤
        private TextBox txtEncodingResult;
        private ComboBox cmbEncodingList;
        private Button btnAnalyzeEncoding;

        // 선택된 파일 데이터 저장 (4.8 버전용 일반 바이트 배열 변수)
        private byte[] fileBytes = null;

        public MainForm()
        {
            // 폼 기본 설정
            this.Text = ".DAT 파일 분석기 (.NET Framework 4.8 버전)";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // 1. 상단 파일 선택 영역
            Label lblFile = new Label();
            lblFile.Text = "파일 경로:";
            lblFile.Location = new Point(15, 20);
            lblFile.Size = new Size(70, 25);

            txtFilePath = new TextBox();
            txtFilePath.Location = new Point(90, 17);
            txtFilePath.Size = new Size(560, 25);
            txtFilePath.ReadOnly = true;

            btnOpenFile = new Button();
            btnOpenFile.Text = "파일 열기";
            btnOpenFile.Location = new Point(660, 15);
            btnOpenFile.Size = new Size(100, 30);
            btnOpenFile.Click += new EventHandler(BtnOpenFile_Click);

            this.Controls.Add(lblFile);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnOpenFile);

            // 2. 탭 컨트롤 구성 (두 가지 케이스 테스트용)
            tabControl = new TabControl();
            tabControl.Location = new Point(15, 60);
            tabControl.Size = new Size(750, 480);

            tabBinary = new TabPage();
            tabBinary.Text = "케이스 1: 숫자/바이너리 분석";

            tabEncoding = new TabPage();
            tabEncoding.Text = "케이스 2: 한자 깨짐/인코딩 변환";

            tabControl.TabPages.Add(tabBinary);
            tabControl.TabPages.Add(tabEncoding);
            this.Controls.Add(tabControl);

            // ==========================================
            // [탭 1] 바이너리 분석 UI 구성
            // ==========================================
            Label lblGroup = new Label();
            lblGroup.Text = "데이터 단위:";
            lblGroup.Location = new Point(15, 20);
            lblGroup.Size = new Size(80, 25);

            cmbByteGroup = new ComboBox();
            cmbByteGroup.Location = new Point(100, 17);
            cmbByteGroup.Size = new Size(150, 25);
            cmbByteGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteGroup.Items.AddRange(new object[] { 
                "1바이트 (Byte)", 
                "2바이트 정수 (Int16)", 
                "4바이트 정수 (Int32)", 
                "4바이트 실수 (Single/Float)", 
                "8바이트 실수 (Double)" 
            });
            cmbByteGroup.SelectedIndex = 2; // 기본값 Int32

            btnAnalyzeBinary = new Button();
            btnAnalyzeBinary.Text = "분석 실행";
            btnAnalyzeBinary.Location = new Point(260, 15);
            btnAnalyzeBinary.Size = new Size(100, 30);
            btnAnalyzeBinary.Enabled = false;
            btnAnalyzeBinary.Click += new EventHandler(BtnAnalyzeBinary_Click);

            txtBinaryResult = new TextBox();
            txtBinaryResult.Location = new Point(15, 60);
            txtBinaryResult.Size = new Size(710, 370);
            txtBinaryResult.Multiline = true;
            txtBinaryResult.ScrollBars = ScrollBars.Vertical;
            txtBinaryResult.Font = new Font("Consolas", 10);

            tabBinary.Controls.Add(lblGroup);
            tabBinary.Controls.Add(cmbByteGroup);
            tabBinary.Controls.Add(btnAnalyzeBinary);
            tabBinary.Controls.Add(txtBinaryResult);

            // ==========================================
            // [탭 2] 인코딩 변환 UI 구성
            // ==========================================
            Label lblEnc = new Label();
            lblEnc.Text = "인코딩 선택:";
            lblEnc.Location = new Point(15, 20);
            lblEnc.Size = new Size(80, 25);

            cmbEncodingList = new ComboBox();
            cmbEncodingList.Location = new Point(100, 17);
            cmbEncodingList.Size = new Size(200, 25);
            cmbEncodingList.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEncodingList.Items.AddRange(new object[] { 
                "모든 인코딩 한 번에 비교", 
                "UTF-8", 
                "UTF-16 LE (Unicode)", 
                "UTF-16 BE", 
                "EUC-KR (옛날 한글 완성형)", 
                "ASCII" 
            });
            cmbEncodingList.SelectedIndex = 0;

            btnAnalyzeEncoding = new Button();
            btnAnalyzeEncoding.Text = "변환 실행";
            btnAnalyzeEncoding.Location = new Point(310, 15);
            btnAnalyzeEncoding.Size = new Size(100, 30);
            btnAnalyzeEncoding.Enabled = false;
            btnAnalyzeEncoding.Click += new EventHandler(BtnAnalyzeEncoding_Click);

            txtEncodingResult = new TextBox();
            txtEncodingResult.Location = new Point(15, 60);
            txtEncodingResult.Size = new Size(710, 370);
            txtEncodingResult.Multiline = true;
            txtEncodingResult.ScrollBars = ScrollBars.Vertical;
            txtEncodingResult.Font = new Font("맑은 고딕", 10);

            tabEncoding.Controls.Add(lblEnc);
            tabEncoding.Controls.Add(cmbEncodingList);
            tabEncoding.Controls.Add(btnAnalyzeEncoding);
            tabEncoding.Controls.Add(txtEncodingResult);
        }

        // 파일 열기 버튼 클릭 이벤트
        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DAT 파일 (*.dat)|*.dat|모든 파일 (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        txtFilePath.Text = openFileDialog.FileName;
                        fileBytes = File.ReadAllBytes(openFileDialog.FileName);

                        MessageBox.Show("파일을 성공적으로 로드했습니다.\n크기: " + fileBytes.Length + " 바이트", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        btnAnalyzeBinary.Enabled = true;
                        btnAnalyzeEncoding.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("파일을 읽는 중 오류 발생: " + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 케이스 1: 바이너리 숫자 분석 실행
        private void BtnAnalyzeBinary_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[분석 결과] 총 " + fileBytes.Length + " 바이트");
            sb.AppendLine("※ 마지막 부분 데이터가 고정되어 있는지 유심히 확인하세요.\n");
            sb.AppendLine("위치(인덱스)\t|\t16진수(Hex)\t|\t해석된 숫자값");
            sb.AppendLine("----------------------------------------------------------------------");

            int step = 4;
            int typeChoice = cmbByteGroup.SelectedIndex;

            switch (typeChoice)
            {
                case 0: step = 1; break; // Byte
                case 1: step = 2; break; // Int16
                case 2: step = 4; break; // Int32
                case 3: step = 4; break; // Single
                case 4: step = 8; break; // Double
            }

            for (int i = 0; i <= fileBytes.Length - step; i += step)
            {
                string hex = "";
                string valueStr = "";

                // Hex 문자열 생성
                for (int j = 0; j < step; j++) 
                {
                    hex += fileBytes[i + j].ToString("X2") + " ";
                }

                // 데이터 해석 (구형 문법 조건문 구조)
                if (typeChoice == 0) valueStr = fileBytes[i].ToString();
                else if (typeChoice == 1) valueStr = BitConverter.ToInt16(fileBytes, i).ToString();
                else if (typeChoice == 2) valueStr = BitConverter.ToInt32(fileBytes, i).ToString();
                else if (typeChoice == 3) valueStr = BitConverter.ToSingle(fileBytes, i).ToString();
                else if (typeChoice == 4) valueStr = BitConverter.ToDouble(fileBytes, i).ToString();

                sb.AppendLine(i + "\t\t|\t" + hex.Trim() + "\t|\t" + valueStr);
            }

            txtBinaryResult.Text = sb.ToString();
        }

        // 케이스 2: 인코딩 변환 실행
        private void BtnAnalyzeEncoding_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            int choice = cmbEncodingList.SelectedIndex;

            if (choice == 0) // 모두 비교
            {
                // .NET 4.8 버전 안전한 Tuple 배열 생성
                string[] names = { "UTF-8", "UTF-16 LE (Unicode)", "UTF-16 BE", "EUC-KR (옛날 한글)", "ASCII (순수 영문)" };
                Encoding[] encodings = { 
                    Encoding.UTF8, 
                    Encoding.Unicode, 
                    Encoding.BigEndianUnicode, 
                    Encoding.GetEncoding("EUC-KR"), 
                    Encoding.ASCII 
                };

                for (int i = 0; i < names.Length; i++)
                {
                    sb.AppendLine("==================================================");
                    sb.AppendLine("▶ 인코딩 타입: " + names[i]);
                    sb.AppendLine("==================================================");
                    sb.AppendLine(encodings[i].GetString(fileBytes));
                    sb.AppendLine("\n");
                }
            }
            else
            {
                Encoding selectedEncoding = Encoding.UTF8;
                switch (choice)
                {
                    case 1: selectedEncoding = Encoding.UTF8; break;
                    case 2: selectedEncoding = Encoding.Unicode; break;
                    case 3: selectedEncoding = Encoding.BigEndianUnicode; break;
                    case 4: selectedEncoding = Encoding.GetEncoding("EUC-KR"); break;
                    case 5: selectedEncoding = Encoding.ASCII; break;
                }
                sb.AppendLine(selectedEncoding.GetString(fileBytes));
            }

            txtEncodingResult.Text = sb.ToString();
        }
    }
}