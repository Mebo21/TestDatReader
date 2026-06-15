using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace DatAnalyzer
{
    static class Program
    {
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
        private Button btnOpenFile;
        private TextBox txtFilePath;
        private TextBox txtCustomMessage;
        private Button btnCreateCustomDat;

        private TabControl tabControl;
        private TabPage tabTextAnalyze;
        private TabPage tabBinaryAnalyze;
        
        // 탭 1 컨트롤 (버튼 2개로 분리)
        private TextBox txtTextResult;
        private Button btnStartEncodingCheck; // 단순 인코딩 확인 버튼
        private Button btnStartXorDecrypt;    // XOR 해독 시도 버튼

        // 탭 2 컨트롤
        private TextBox txtBinaryResult;
        private ComboBox cmbByteGroup;
        private Button btnStartBinaryExtract;

        private byte[] fileBytes = null;

        public MainForm()
        {
            this.Text = "DAT 파일 데이터 구조 분석 및 복원 시스템 (v1.1)";
            this.Size = new Size(920, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);

            InitializeControls();
        }

        private void InitializeControls()
        {
            Font standardFont = new Font("맑은 고딕", 9F, FontStyle.Regular);
            Font boldFont = new Font("맑은 고딕", 9F, FontStyle.Bold);

            // 1. 상단 파일 선택
            Label lblFile = new Label();
            lblFile.Text = "대상 파일 경로:";
            lblFile.Location = new Point(20, 22);
            lblFile.Size = new Size(90, 25);
            lblFile.Font = standardFont;

            txtFilePath = new TextBox();
            txtFilePath.Location = new Point(115, 19);
            txtFilePath.Size = new Size(640, 25);
            txtFilePath.Font = standardFont;
            txtFilePath.ReadOnly = true;

            btnOpenFile = new Button();
            btnOpenFile.Text = "파일 열기";
            btnOpenFile.Location = new Point(765, 17);
            btnOpenFile.Size = new Size(110, 28);
            btnOpenFile.Font = standardFont;
            btnOpenFile.Click += new EventHandler(BtnOpenFile_Click);

            this.Controls.Add(lblFile);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnOpenFile);

            // 2. 상단 테스트 데이터 생성기
            GroupBox grpCreate = new GroupBox();
            grpCreate.Text = " 테스트 데이터 파일 생성 (검증용) ";
            grpCreate.Location = new Point(20, 55);
            grpCreate.Size = new Size(855, 65);
            grpCreate.Font = boldFont;
            
            Label lblMsg = new Label();
            lblMsg.Text = "주입 문장 입력:";
            lblMsg.Location = new Point(15, 28);
            lblMsg.Size = new Size(100, 25);
            lblMsg.Font = standardFont;

            txtCustomMessage = new TextBox();
            txtCustomMessage.Location = new Point(115, 25);
            txtCustomMessage.Size = new Size(530, 25);
            txtCustomMessage.Font = standardFont;
            txtCustomMessage.Text = "[DATA] Test 테스트테스트 9991123 21460769 추적번호 ABCD_ @!s?";

            btnCreateCustomDat = new Button();
            btnCreateCustomDat.Text = "테스트 DAT 생성";
            btnCreateCustomDat.Location = new Point(660, 23);
            btnCreateCustomDat.Size = new Size(175, 28);
            btnCreateCustomDat.Font = standardFont;
            btnCreateCustomDat.Click += new EventHandler(BtnCreateCustomDat_Click);

            grpCreate.Controls.Add(lblMsg);
            grpCreate.Controls.Add(txtCustomMessage);
            grpCreate.Controls.Add(btnCreateCustomDat);
            this.Controls.Add(grpCreate);

            // 3. 메인 기능 탭
            tabControl = new TabControl();
            tabControl.Location = new Point(20, 135);
            tabControl.Size = new Size(855, 480);
            tabControl.Font = standardFont;

            tabTextAnalyze = new TabPage();
            tabTextAnalyze.Text = " 텍스트 복원 및 해독 분석 ";

            tabBinaryAnalyze = new TabPage();
            tabBinaryAnalyze.Text = " 순수 숫자 데이터 추출 ";

            tabControl.TabPages.Add(tabTextAnalyze);
            tabControl.TabPages.Add(tabBinaryAnalyze);
            this.Controls.Add(tabControl);

            // ==========================================
            // [기능 탭 1] 두 개의 분석 버튼 독립 배치
            // ==========================================
            btnStartEncodingCheck = new Button();
            btnStartEncodingCheck.Text = "단순 인코딩 확인";
            btnStartEncodingCheck.Location = new Point(15, 15);
            btnStartEncodingCheck.Size = new Size(160, 30);
            btnStartEncodingCheck.Font = boldFont;
            btnStartEncodingCheck.Enabled = false;
            btnStartEncodingCheck.Click += new EventHandler(btnStartEncodingCheck_Click);

            btnStartXorDecrypt = new Button();
            btnStartXorDecrypt.Text = "XOR 해독 시도";
            btnStartXorDecrypt.Location = new Point(185, 15);
            btnStartXorDecrypt.Size = new Size(160, 30);
            btnStartXorDecrypt.Font = boldFont;
            btnStartXorDecrypt.Enabled = false;
            btnStartXorDecrypt.Click += new EventHandler(btnStartXorDecrypt_Click);

            txtTextResult = new TextBox();
            txtTextResult.Location = new Point(15, 55);
            txtTextResult.Size = new Size(820, 390);
            txtTextResult.Multiline = true;
            txtTextResult.ScrollBars = ScrollBars.Vertical;
            txtTextResult.Font = new Font("Consolas", 10F, FontStyle.Regular);
            txtTextResult.BackColor = Color.White;
            txtTextResult.ReadOnly = true;

            tabTextAnalyze.Controls.Add(btnStartEncodingCheck);
            tabTextAnalyze.Controls.Add(btnStartXorDecrypt);
            tabTextAnalyze.Controls.Add(txtTextResult);

            // ==========================================
            // [기능 탭 2] 순수 숫자 데이터 추출 영역
            // ==========================================
            Label lblGroup = new Label();
            lblGroup.Text = "데이터 분석 단위:";
            lblGroup.Location = new Point(15, 18);
            lblGroup.Size = new Size(110, 25);
            lblGroup.Font = standardFont;

            cmbByteGroup = new ComboBox();
            cmbByteGroup.Location = new Point(125, 15);
            cmbByteGroup.Size = new Size(180, 25);
            cmbByteGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteGroup.Font = standardFont;
            cmbByteGroup.Items.AddRange(new object[] { 
                "1바이트 정수 (Byte)", 
                "2바이트 정수 (Int16)", 
                "4바이트 정수 (Int32)", 
                "4바이트 실수 (Float/Single)", 
                "8바이트 실수 (Double)" 
            });
            cmbByteGroup.SelectedIndex = 2;

            btnStartBinaryExtract = new Button();
            btnStartBinaryExtract.Text = "숫자 데이터 추출 및 해독";
            btnStartBinaryExtract.Location = new Point(320, 13);
            btnStartBinaryExtract.Size = new Size(200, 28);
            btnStartBinaryExtract.Font = boldFont;
            btnStartBinaryExtract.Enabled = false;
            btnStartBinaryExtract.Click += new EventHandler(btnStartBinaryExtract_Click);

            txtBinaryResult = new TextBox();
            txtBinaryResult.Location = new Point(15, 55);
            txtBinaryResult.Size = new Size(820, 390);
            txtBinaryResult.Multiline = true;
            txtBinaryResult.ScrollBars = ScrollBars.Vertical;
            txtBinaryResult.Font = new Font("Consolas", 10F, FontStyle.Regular);
            txtBinaryResult.BackColor = Color.White;
            txtBinaryResult.ReadOnly = true;

            tabBinaryAnalyze.Controls.Add(lblGroup);
            tabBinaryAnalyze.Controls.Add(cmbByteGroup);
            tabBinaryAnalyze.Controls.Add(btnStartBinaryExtract);
            tabBinaryAnalyze.Controls.Add(txtBinaryResult);
        }

        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                openFileDialog.Filter = "DAT 파일 (*.dat)|*.dat|모든 파일 (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        txtFilePath.Text = openFileDialog.FileName;
                        fileBytes = File.ReadAllBytes(openFileDialog.FileName);

                        btnStartEncodingCheck.Enabled = true;
                        btnStartXorDecrypt.Enabled = true;
                        btnStartBinaryExtract.Enabled = true;
                        
                        txtTextResult.Clear();
                        txtBinaryResult.Clear();
                        
                        MessageBox.Show("파일이 로드되었습니다. (" + fileBytes.Length + " 바이트)", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show("파일 읽기 오류: " + ex.Message, "오류"); }
                }
            }
        }

        private void BtnCreateCustomDat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomMessage.Text.Trim())) return;

            string customPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_test.dat");
            try
            {
                using (FileStream fs = new FileStream(customPath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] userTextBytes = Encoding.GetEncoding(949).GetBytes(txtCustomMessage.Text);
                    
                    // 분석 검증용 0x5A 키 고정 암호화 주입
                    byte xorKey = 0x5A; 
                    for (int i = 0; i < userTextBytes.Length; i++)
                    {
                        userTextBytes[i] = (byte)(userTextBytes[i] ^ xorKey);
                    }

                    bw.Write(userTextBytes);
                    for (int i = 0; i < 5; i++) bw.Write(999);
                    bw.Write((long)0);
                }
                MessageBox.Show("검증용 데이터 파일(custom_test.dat)이 생성되었습니다.", "완료");
            }
            catch (Exception ex) { MessageBox.Show("생성 실패: " + ex.Message); }
        }

        // ==========================================
        // [신규 분리 1] 단순 인코딩 확인 전용 로직
        // ==========================================
        private void btnStartEncodingCheck_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[텍스트 데이터 변환 결과 보고서]");
            sb.AppendLine("분석 유형: 표준 인코딩 매칭 분석");
            sb.AppendLine("----------------------------------------------------------------------");

            string[] encNames = { "UTF-8", "CP949 (EUC-KR)", "UTF-16 LE" };
            Encoding[] encs = { Encoding.UTF8, Encoding.GetEncoding(949), Encoding.Unicode };
            bool anyMatch = false;

            for (int i = 0; i < encs.Length; i++)
            {
                string testDecoded = encs[i].GetString(fileBytes).Replace("\0", "").Trim();
                
                sb.AppendLine("적용 인코딩: " + encNames[i]);
                sb.AppendLine("출력 데이터: " + testDecoded);
                sb.AppendLine("----------------------------------------------------------------------");
                anyMatch = true;
            }

            txtTextResult.Text = sb.ToString();
        }

        // ==========================================
        // [신규 분리 2] XOR 암호화 해독 시도 전용 로직 (출력 간소화)
        // ==========================================
        private void btnStartXorDecrypt_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[바이너리 암호화 해독 결과 보고서]");
            sb.AppendLine("분석 유형: 0~255 무차별 대입 복호화 (XOR Brute Force)");
            sb.AppendLine("----------------------------------------------------------------------");

            bool success = false;

            for (int key = 1; key <= 255; key++)
            {
                byte[] tempBytes = new byte[fileBytes.Length];
                Array.Copy(fileBytes, tempBytes, fileBytes.Length);

                for (int i = 0; i < tempBytes.Length; i++)
                {
                    tempBytes[i] = (byte)(tempBytes[i] ^ key);
                }

                // 한국어 윈도우 표준 환경 기반으로 복구 테스트
                string decodedStr = Encoding.GetEncoding(949).GetString(tempBytes).Replace("\0", "").Trim();

                if (IsReadableText(decodedStr))
                {
                    // 불필요한 수식어를 제거하고 간결하게 텍스트(매칭된 비밀키) 형태로 표기
                    sb.AppendLine(string.Format("복원 텍스트 (매칭 비밀키: 0x{0:X2} / 10진수: {1})", key, key));
                    sb.AppendLine("출력 데이터: " + decodedStr);
                    sb.AppendLine("----------------------------------------------------------------------");
                    success = true;
                    break; // 유효 문장이 나타나면 즉시 중단하여 불필요한 깨진 문자열 배제
                }
            }

            if (!success)
            {
                sb.AppendLine("[결과 보고] 파일의 데이터셋 구조와 일치하는 암호화 키를 발견하지 못했습니다.");
            }

            txtTextResult.Text = sb.ToString();
        }

        private void btnStartBinaryExtract_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[바이너리 수치 데이터 정밀 추출 보고서]");
            sb.AppendLine("----------------------------------------------------------------------");
            sb.AppendLine("데이터 주소 (Address)\t|\t바이너리 (Hex)\t|\t해석된 수치 데이터");
            sb.AppendLine("----------------------------------------------------------------------");

            int step = 4;
            int typeChoice = cmbByteGroup.SelectedIndex;

            switch (typeChoice)
            {
                case 0: step = 1; break;
                case 1: step = 2; break;
                case 2: step = 4; break;
                case 3: step = 4; break;
                case 4: step = 8; break;
            }

            int validCount = 0;

            for (int i = 0; i <= fileBytes.Length - step; i += step)
            {
                string hex = "";
                double numericValue = 0;

                for (int j = 0; j < step; j++) 
                    hex += fileBytes[i + j].ToString("X2") + " ";

                if (typeChoice == 0) numericValue = fileBytes[i];
                else if (typeChoice == 1) numericValue = BitConverter.ToInt16(fileBytes, i);
                else if (typeChoice == 2) numericValue = BitConverter.ToInt32(fileBytes, i);
                else if (typeChoice == 3) numericValue = BitConverter.ToSingle(fileBytes, i);
                else if (typeChoice == 4) numericValue = BitConverter.ToDouble(fileBytes, i);

                if (numericValue < 0 || numericValue == 0) continue;

                sb.AppendLine(i + "\t\t\t|\t" + hex.Trim() + "\t|\t" + numericValue);
                validCount++;
            }

            if (validCount == 0)
            {
                sb.AppendLine("[안내] 현재 선택된 데이터 단위(바이트) 내에서는 유효한 정수/실수 값을 추출하지 못했습니다.");
            }

            txtBinaryResult.Text = sb.ToString();
        }

        private bool IsReadableText(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            
            int validChars = 0;
            foreach (char c in input)
            {
                if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ' || c == '[' || c == ']' || c == '?' || c == '@' || c == '!')
                {
                    validChars++;
                }
            }
            return validChars > 8;
        }
    }
}