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
        // 상단 파일 및 커스텀 생성 컨트롤
        private Button btnOpenFile;
        private TextBox txtFilePath;
        private TextBox txtCustomMessage;
        private Button btnCreateCustomDat;

        // 탭 컨트롤 및 페이지
        private TabControl tabControl;
        private TabPage tabBinary;
        private TabPage tabEncoding;
        
        // 탭 1: 바이너리 숫자 추출용
        private TextBox txtBinaryResult;
        private ComboBox cmbByteGroup;
        private Button btnAnalyzeBinary;

        // 탭 2: 난독화 자동 해제 및 인코딩 분석용
        private TextBox txtEncodingResult;
        private Button btnAnalyzeEncoding;

        private byte[] fileBytes = null;

        public MainForm()
        {
            this.Text = ".DAT 난독화 강제 해제 및 데이터 정밀 정제기 (.NET 4.8)";
            this.Size = new Size(880, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // 1. 첫 번째 라인: 파일 로드 레이아웃
            Label lblFile = new Label();
            lblFile.Text = "대상 파일:";
            lblFile.Location = new Point(15, 20);
            lblFile.Size = new Size(65, 25);

            txtFilePath = new TextBox();
            txtFilePath.Location = new Point(85, 17);
            txtFilePath.Size = new Size(640, 25);
            txtFilePath.ReadOnly = true;

            btnOpenFile = new Button();
            btnOpenFile.Text = "파일 열기";
            btnOpenFile.Location = new Point(740, 15);
            btnOpenFile.Size = new Size(100, 30);
            btnOpenFile.Click += new EventHandler(BtnOpenFile_Click);

            this.Controls.Add(lblFile);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnOpenFile);

            // 2. 두 번째 라인: 난독화 테스트용 DAT 생성기 영역
            GroupBox grpCreate = new GroupBox();
            grpCreate.Text = " 진짜 깨지는 난독화 DAT 파일 생성기 (XOR 암호화 적용) ";
            grpCreate.Location = new Point(15, 55);
            grpCreate.Size = new Size(825, 65);
            
            Label lblMsg = new Label();
            lblMsg.Text = "숨길 메시지 입력:";
            lblMsg.Location = new Point(15, 28);
            lblMsg.Size = new Size(110, 25);

            txtCustomMessage = new TextBox();
            txtCustomMessage.Location = new Point(130, 25);
            txtCustomMessage.Size = new Size(500, 25);
            txtCustomMessage.Text = "[DATA] Test 테스트테스트 9991123 21460769 추적번호 ABCD_ @!s?";

            btnCreateCustomDat = new Button();
            btnCreateCustomDat.Text = "암호화 DAT 굽기";
            btnCreateCustomDat.Location = new Point(645, 22);
            btnCreateCustomDat.Size = new Size(165, 30);
            btnCreateCustomDat.BackColor = Color.LightCoral;
            btnCreateCustomDat.ForeColor = Color.White;
            btnCreateCustomDat.Font = new Font(this.Font, FontStyle.Bold);
            btnCreateCustomDat.Click += new EventHandler(BtnCreateCustomDat_Click);

            grpCreate.Controls.Add(lblMsg);
            grpCreate.Controls.Add(txtCustomMessage);
            grpCreate.Controls.Add(btnCreateCustomDat);
            this.Controls.Add(grpCreate);

            // 3. 메인 분석 탭 컨트롤 구성
            tabControl = new TabControl();
            tabControl.Location = new Point(15, 135);
            tabControl.Size = new Size(825, 480);

            tabBinary = new TabPage();
            tabBinary.Text = "[필터링] 바이너리 알맹이 숫자 추출";

            tabEncoding = new TabPage();
            tabEncoding.Text = "[해제공격] 난독화 해제 및 문자열 복원";

            tabControl.TabPages.Add(tabBinary);
            tabControl.TabPages.Add(tabEncoding);
            this.Controls.Add(tabControl);

            // [탭 1] 바이너리 숫자 추출 상세 디자인
            Label lblGroup = new Label();
            lblGroup.Text = "데이터 단위 정밀 선택:";
            lblGroup.Location = new Point(15, 20);
            lblGroup.Size = new Size(140, 25);

            cmbByteGroup = new ComboBox();
            cmbByteGroup.Location = new Point(160, 17);
            cmbByteGroup.Size = new Size(180, 25);
            cmbByteGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteGroup.Items.AddRange(new object[] { 
                "1바이트 정수 (Byte)", 
                "2바이트 정수 (Int16)", 
                "4바이트 정수 (Int32)", 
                "4바이트 실수 (Float/Single)", 
                "8바이트 실수 (Double)" 
            });
            cmbByteGroup.SelectedIndex = 2; // 기본 Int32

            btnAnalyzeBinary = new Button();
            btnAnalyzeBinary.Text = "유령값 청소하고 진짜 숫자 데이터만 추출";
            btnAnalyzeBinary.Location = new Point(350, 15);
            btnAnalyzeBinary.Size = new Size(240, 30);
            btnAnalyzeBinary.Enabled = false;
            btnAnalyzeBinary.Click += new EventHandler(BtnAnalyzeBinary_Click);

            txtBinaryResult = new TextBox();
            txtBinaryResult.Location = new Point(15, 60);
            txtBinaryResult.Size = new Size(790, 380);
            txtBinaryResult.Multiline = true;
            txtBinaryResult.ScrollBars = ScrollBars.Vertical;
            txtBinaryResult.Font = new Font("Consolas", 10);

            tabBinary.Controls.Add(lblGroup);
            tabBinary.Controls.Add(cmbByteGroup);
            tabBinary.Controls.Add(btnAnalyzeBinary);
            tabBinary.Controls.Add(txtBinaryResult);

            // [탭 2] 난독화 강제 해제 공격 디자인
            btnAnalyzeEncoding = new Button();
            btnAnalyzeEncoding.Text = "★ 0~255 전수조사 난독화 파쇄 및 텍스트 자동 복원 실행 ★";
            btnAnalyzeEncoding.Location = new Point(15, 15);
            btnAnalyzeEncoding.Size = new Size(420, 30);
            btnAnalyzeEncoding.BackColor = Color.LightGreen;
            btnAnalyzeEncoding.Font = new Font(this.Font, FontStyle.Bold);
            btnAnalyzeEncoding.Enabled = false;
            btnAnalyzeEncoding.Click += new EventHandler(BtnAnalyzeEncoding_Click);

            txtEncodingResult = new TextBox();
            txtEncodingResult.Location = new Point(15, 60);
            txtEncodingResult.Size = new Size(790, 380);
            txtEncodingResult.Multiline = true;
            txtEncodingResult.ScrollBars = ScrollBars.Vertical;
            txtEncodingResult.Font = new Font("맑은 고딕", 10);

            tabEncoding.Controls.Add(btnAnalyzeEncoding);
            tabEncoding.Controls.Add(txtEncodingResult);
        }

        // 1. 진짜로 꼬여서 메모장에서 외계어로 깨지는 암호화 DAT 생성 로직
        private void BtnCreateCustomDat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomMessage.Text.Trim()))
            {
                MessageBox.Show("메시지를 입력해 주세요!", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string customPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_test.dat");
            try
            {
                using (FileStream fs = new FileStream(customPath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    // 한국어 완성형 바이트 변환
                    byte[] userTextBytes = Encoding.GetEncoding(949).GetBytes(txtCustomMessage.Text);

                    // 🔥 [진짜 난독화 주입] 0x5A(비밀 키)로 비트 XOR 연산을 수행하여 바이트 자체를 훼손시킵니다.
                    // 이렇게 하면 메모장이나 일반 뷰어로 열었을 때 100% 한자나 외계어로 깨집니다.
                    byte xorKey = 0x5A; 
                    for (int i = 0; i < userTextBytes.Length; i++)
                    {
                        userTextBytes[i] = (byte)(userTextBytes[i] ^ xorKey);
                    }

                    // 난독화된 바이트 쓰기
                    bw.Write(userTextBytes);

                    // 데이터 영역 뒤에 숨겨진 진짜 규칙 숫자 주입 (999)
                    for (int i = 0; i < 5; i++) bw.Write(999);
                    
                    // 빈 공간 패딩 0 주입
                    bw.Write((long)0);
                }

                MessageBox.Show("난독화(XOR 암호화)가 완료된 'custom_test.dat' 파일이 정상 생성되었습니다!\n\n이 파일을 메모장으로 열어서 깨진 상태를 먼저 확인해보세요.", "생성 성공");
            }
            catch (Exception ex) 
            { 
                MessageBox.Show("파일 생성 중 예외 발생: " + ex.Message, "오류"); 
            }
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
                        MessageBox.Show("데이터 분석 준비 완료: " + fileBytes.Length + " 바이트 로드됨", "성공");

                        btnAnalyzeBinary.Enabled = true;
                        btnAnalyzeEncoding.Enabled = true;
                    }
                    catch (Exception ex) { MessageBox.Show("파일 열기 실패: " + ex.Message); }
                }
            }
        }

        // 2. 바이너리 탭 로직: 유령 수치들을 지우고 유효한 숫자 데이터셋만 트래킹
        private void BtnAnalyzeBinary_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("======================================================================");
            sb.AppendLine(" [바이너리 정제] 비정상적인 유령 마이너스 값 및 빈 공간 패딩 0을 제거한 핵심 결과");
            sb.AppendLine("======================================================================");
            sb.AppendLine("데이터 주소\t|\t16진수 바이너리 코드\t|\t정제된 실제 데이터 값");
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

                // 필터링: 난독화나 깨짐 현상 때문에 비정상적으로 튀는 마이너스(-)값 및 의미없는 빈 공간 0 데이터 제외
                if (numericValue < 0 || numericValue == 0) continue;

                sb.AppendLine(i + "\t\t|\t" + hex.Trim() + "\t|\t" + numericValue);
                validCount++;
            }

            if (validCount == 0)
            {
                sb.AppendLine("\n[검출 데이터 없음] 현재 단위로는 유효한 숫자값을 발견하지 못했습니다.");
                sb.AppendLine("데이터 단위를 변경해가며 다시 시도해보세요.");
            }

            txtBinaryResult.Text = sb.ToString();
        }

        // 3. 난독화 파쇄 탭 로직: 0~255 비밀 키 브루트포스 대입 연산으로 진짜 문장을 자동 복원
        private void BtnAnalyzeEncoding_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("======================================================================");
            sb.AppendLine(" [난독화 무력화 결과] 0 ~ 255 전체 비밀 복호화 키 무차별 검증 결과");
            sb.AppendLine("======================================================================");
            sb.AppendLine("※ 수많은 외계어 조합을 연산 필터로 걸러내고, 진짜 텍스트 문장이 검출된 구간만 표시합니다.\n");

            int matchCount = 0;

            // 0부터 255까지 모든 바이트 키값을 순식간에 대입해 연산합니다.
            for (int key = 0; key <= 255; key++)
            {
                byte[] tempBytes = new byte[fileBytes.Length];
                Array.Copy(fileBytes, tempBytes, fileBytes.Length);

                // 현재 루프의 키값으로 역 XOR 연산 진행
                for (int i = 0; i < tempBytes.Length; i++)
                {
                    tempBytes[i] = (byte)(tempBytes[i] ^ key);
                }

                // 가장 흔한 윈도우 한국어 인코딩(CP949) 방식으로 문자열 변환 시도
                string decodedStr = Encoding.GetEncoding(949).GetString(tempBytes).Replace("\0", "").Trim();

                // 가독성 지표 계산 가동 (문자열 내부에 정상적인 한글, 영어, 숫자가 얼마나 포함되어 있는가?)
                int readabilityScore = 0;
                foreach (char c in decodedStr)
                {
                    if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ' || c == '[' || c == ']')
                    {
                        readabilityScore++;
                    }
                }

                // 전체 데이터 중 의미 있는 글자가 6글자 이상 매칭되는 정상 복원 키값만 화면에 추출
                if (readabilityScore > 6)
                {
                    sb.AppendLine("----------------------------------------------------------------------");
                    sb.AppendLine(string.Format("★ 난독화 무력화 성공 후보 발견!! -> 매칭된 비밀 키(XOR Key): 0x{0:X2} (10진수: {1})", key, key));
                    sb.AppendLine("----------------------------------------------------------------------");
                    sb.AppendLine("[해독된 실제 원본 텍스트 데이터]:");
                    sb.AppendLine(decodedStr);
                    sb.AppendLine();
                    matchCount++;
                }
            }

            if (matchCount == 0)
            {
                sb.AppendLine("\n[분석 실패] 단순 1바이트 XOR 방식의 난독화 파일이 아니거나, 완전히 다른 암호화 기법이 적용된 파일일 수 있습니다.");
            }

            txtEncodingResult.Text = sb.ToString();
        }
    }
}