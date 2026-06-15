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
        // 상단 파일 제어 컨트롤
        private Button btnOpenFile;
        private TextBox txtFilePath;
        private TextBox txtCustomMessage;
        private Button btnCreateCustomDat;

        // 메인 탭 컨트롤
        private TabControl tabControl;
        private TabPage tabTextAnalyze;
        private TabPage tabBinaryAnalyze;
        
        // 탭 1: 텍스트 및 난독화 해독 영역
        private TextBox txtTextResult;
        private Button btnStartTextDecrypt;

        // 탭 2: 순수 숫자 데이터 추출 영역
        private TextBox txtBinaryResult;
        private ComboBox cmbByteGroup;
        private Button btnStartBinaryExtract;

        private byte[] fileBytes = null;

        public MainForm()
        {
            // 공적인 자리에 적합한 단정하고 표준적인 타이틀 명명
            this.Text = "DAT 파일 데이터 구조 분석 및 복원 시스템 (v1.0)";
            this.Size = new Size(900, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245); // 차분한 기업형 배경색

            InitializeControls();
        }

        private void InitializeControls()
        {
            Font standardFont = new Font("맑은 고딕", 9F, FontStyle.Regular);
            Font boldFont = new Font("맑은 고딕", 9F, FontStyle.Bold);

            // 1. 상단: 파일 선택 영역
            Label lblFile = new Label();
            lblFile.Text = "대상 파일 경로:";
            lblFile.Location = new Point(20, 22);
            lblFile.Size = new Size(90, 25);
            lblFile.Font = standardFont;

            txtFilePath = new TextBox();
            txtFilePath.Location = new Point(115, 19);
            txtFilePath.Size = new Size(620, 25);
            txtFilePath.Font = standardFont;
            txtFilePath.ReadOnly = true;

            btnOpenFile = new Button();
            btnOpenFile.Text = "파일 열기";
            btnOpenFile.Location = new Point(745, 17);
            btnOpenFile.Size = new Size(110, 28);
            btnOpenFile.Font = standardFont;
            btnOpenFile.Click += new EventHandler(BtnOpenFile_Click);

            this.Controls.Add(lblFile);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnOpenFile);

            // 2. 상단: 테스트 데이터 생성 영역 (깔끔한 그레이톤 프레임)
            GroupBox grpCreate = new GroupBox();
            grpCreate.Text = " 테스트 데이터 파일 생성 (검증용) ";
            grpCreate.Location = new Point(20, 55);
            grpCreate.Size = new Size(835, 65);
            grpCreate.Font = boldFont;
            
            Label lblMsg = new Label();
            lblMsg.Text = "주입 문장 입력:";
            lblMsg.Location = new Point(15, 28);
            lblMsg.Size = new Size(100, 25);
            lblMsg.Font = standardFont;

            txtCustomMessage = new TextBox();
            txtCustomMessage.Location = new Point(115, 25);
            txtCustomMessage.Size = new Size(510, 25);
            txtCustomMessage.Font = standardFont;
            txtCustomMessage.Text = "[DATA] Test 테스트테스트 9991123 21460769 추적번호 ABCD_ @!s?";

            btnCreateCustomDat = new Button();
            btnCreateCustomDat.Text = "테스트 DAT 생성";
            btnCreateCustomDat.Location = new Point(640, 23);
            btnCreateCustomDat.Size = new Size(175, 28);
            btnCreateCustomDat.Font = standardFont;
            btnCreateCustomDat.Click += new EventHandler(BtnCreateCustomDat_Click);

            grpCreate.Controls.Add(lblMsg);
            grpCreate.Controls.Add(txtCustomMessage);
            grpCreate.Controls.Add(btnCreateCustomDat);
            this.Controls.Add(grpCreate);

            // 3. 메인 기능 탭 컨트롤 레이아웃
            tabControl = new TabControl();
            tabControl.Location = new Point(20, 135);
            tabControl.Size = new Size(835, 480);
            tabControl.Font = standardFont;

            tabTextAnalyze = new TabPage();
            tabTextAnalyze.Text = " 텍스트 복원 및 해독 분석 ";

            tabBinaryAnalyze = new TabPage();
            tabBinaryAnalyze.Text = " 순수 숫자 데이터 추출 ";

            tabControl.TabPages.Add(tabTextAnalyze);
            tabControl.TabPages.Add(tabBinaryAnalyze);
            this.Controls.Add(tabControl);

            // ==========================================
            // [기능 탭 1] 텍스트 복원 및 해독 디자인 (단순 인코딩 + XOR 전수조사 통합)
            // ==========================================
            btnStartTextDecrypt = new Button();
            btnStartTextDecrypt.Text = "데이터 해독 시도";
            btnStartTextDecrypt.Location = new Point(15, 15);
            btnStartTextDecrypt.Size = new Size(180, 30);
            btnStartTextDecrypt.Font = boldFont;
            btnStartTextDecrypt.Enabled = false;
            btnStartTextDecrypt.Click += new EventHandler(btnStartTextDecrypt_Click);

            txtTextResult = new TextBox();
            txtTextResult.Location = new Point(15, 55);
            txtTextResult.Size = new Size(800, 390);
            txtTextResult.Multiline = true;
            txtTextResult.ScrollBars = ScrollBars.Vertical;
            txtTextResult.Font = new Font("Consolas", 10F, FontStyle.Regular);
            txtTextResult.BackColor = Color.White;
            txtTextResult.ReadOnly = true;

            tabTextAnalyze.Controls.Add(btnStartTextDecrypt);
            tabTextAnalyze.Controls.Add(txtTextResult);

            // ==========================================
            // [기능 탭 2] 순수 숫자 데이터 추출 디자인 (바이너리 해독 시스템)
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
            cmbByteGroup.SelectedIndex = 2; // 기본 4바이트 정수

            btnStartBinaryExtract = new Button();
            btnStartBinaryExtract.Text = "숫자 데이터 추출 및 해독";
            btnStartBinaryExtract.Location = new Point(320, 13);
            btnStartBinaryExtract.Size = new Size(200, 28);
            btnStartBinaryExtract.Font = boldFont;
            btnStartBinaryExtract.Enabled = false;
            btnStartBinaryExtract.Click += new EventHandler(btnStartBinaryExtract_Click);

            txtBinaryResult = new TextBox();
            txtBinaryResult.Location = new Point(15, 55);
            txtBinaryResult.Size = new Size(800, 390);
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

                        btnStartTextDecrypt.Enabled = true;
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
                    
                    // 분석 테스트를 위해 0x5A 키로 임의 훼손(XOR 난독화) 적용하여 저장
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
        // [개편된 로직 1] 단순 인코딩 확인 및 난독화 해독 통합 수행
        // ==========================================
        private void btnStartTextDecrypt_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[텍스트 데이터 해독 시도 보고서]");
            sb.AppendLine("분석 일시: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("----------------------------------------------------------------------");

            bool textFound = false;

            // 단계 1: 단순 인코딩 문제인지 먼저 점검 (비밀키 0 상태의 정상 변환 확인)
            string[] encNames = { "UTF-8", "CP949 (EUC-KR)", "UTF-16 LE" };
            Encoding[] encs = { Encoding.UTF8, Encoding.GetEncoding(949), Encoding.Unicode };

            for (int i = 0; i < encs.Length; i++)
            {
                string testDecoded = encs[i].GetString(fileBytes).Replace("\0", "").Trim();
                if (IsReadableText(testDecoded))
                {
                    sb.AppendLine("[검출 성공] 단순 인코딩 변환 매칭 확인");
                    sb.AppendLine("적용 인코딩: " + encNames[i]);
                    sb.AppendLine("출력 데이터: " + testDecoded);
                    sb.AppendLine("----------------------------------------------------------------------");
                    textFound = true;
                    break;
                }
            }

            // 단계 2: 단순 인코딩으로 안 풀릴 경우, XOR 암호화 전수 대조 실행
            if (!textFound)
            {
                for (int key = 1; key <= 255; key++) // 0은 위에서 검사했으므로 1부터 진행
                {
                    byte[] tempBytes = new byte[fileBytes.Length];
                    Array.Copy(fileBytes, tempBytes, fileBytes.Length);

                    for (int i = 0; i < tempBytes.Length; i++)
                    {
                        tempBytes[i] = (byte)(tempBytes[i] ^ key);
                    }

                    string decodedStr = Encoding.GetEncoding(949).GetString(tempBytes).Replace("\0", "").Trim();

                    if (IsReadableText(decodedStr))
                    {
                        sb.AppendLine("[해독 성공] 암호화 데이터 복원 완료");
                        sb.AppendLine("매칭된 비밀키 (XOR Key): 0x" + key.ToString("X2") + " (10진수: " + key + ")");
                        sb.AppendLine("출력 데이터: " + decodedStr);
                        sb.AppendLine("----------------------------------------------------------------------");
                        textFound = true;
                        break; // 가장 유효한 키 하나만 잡히면 즉시 정지하여 불필요한 외계어 출력 방지
                    }
                }
            }

            if (!textFound)
            {
                sb.AppendLine("[분석 결과] 텍스트 유형의 문자열 데이터를 검출하지 못했습니다.");
                sb.AppendLine("이 파일은 텍스트 정보가 배제된 순수 수치 포맷 바이너리 파일일 수 있습니다.");
            }

            txtTextResult.Text = sb.ToString();
        }

        // ==========================================
        // [개편된 로직 2] 순수 숫자로만 되어있는 구간 파싱 및 정밀 정제 해독
        // ==========================================
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

                // 유효 필터링 처리 (텍스트가 깨져 나오는 마이너스 불량값 및 의미 없는 0 패딩 생략)
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

        // 유 의미한 정상 문장인지 점수 검증하는 내부 모듈
        private bool IsReadableText(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            
            int validChars = 0;
            foreach (char c in input)
            {
                // 한글 영역, 영문 대소문자, 숫자, 제어 대괄호, 공백 등 정상 데이터셋 체크
                if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ' || c == '[' || c == ']' || c == '?' || c == '@' || c == '!')
                {
                    validChars++;
                }
            }
            // 가독성 있는 글자 수가 최소 8자 이상 매칭될 때만 유효 텍스트로 인정
            return validChars > 8;
        }
    }
}