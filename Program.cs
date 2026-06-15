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
        // 파일 로드 및 상단 컨트롤
        private Button btnOpenFile;
        private TextBox txtFilePath;
        
        // [추가] 커스텀 DAT 생성 컨트롤 영역
        private TextBox txtCustomMessage;
        private Button btnCreateCustomDat;

        // 탭 구성
        private TabControl tabControl;
        private TabPage tabBinary;
        private TabPage tabEncoding;
        
        // 탭 1: 바이너리
        private TextBox txtBinaryResult;
        private ComboBox cmbByteGroup;
        private Button btnAnalyzeBinary;

        // 탭 2: 인코딩
        private TextBox txtEncodingResult;
        private Button btnAnalyzeEncoding;

        private byte[] fileBytes = null;

        public MainForm()
        {
            this.Text = ".DAT 파일 맞춤형 텍스트-바이너리 정밀 분석기 (.NET 4.8)";
            this.Size = new Size(880, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // 1. 상단 라인: 파일 열기 영역
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

            // 2. [신규 추가] 두 번째 라인: 내가 적는 메세지로 DAT 만들기 그룹 박스
            GroupBox grpCreate = new GroupBox();
            grpCreate.Text = " 커스텀 테스트 DAT 파일 생성기 ";
            grpCreate.Location = new Point(15, 55);
            grpCreate.Size = new Size(825, 65);
            
            Label lblMsg = new Label();
            lblMsg.Text = "숨길 메시지 입력:";
            lblMsg.Location = new Point(15, 28);
            lblMsg.Size = new Size(110, 25);

            txtCustomMessage = new TextBox();
            txtCustomMessage.Location = new Point(130, 25);
            txtCustomMessage.Size = new Size(500, 25);
            txtCustomMessage.Text = "내가 직접 입력한 비밀 메시지입니다! 심지어 숫자 999도 숨어있지롱";

            btnCreateCustomDat = new Button();
            btnCreateCustomDat.Text = "이 메세지로 DAT 굽기";
            btnCreateCustomDat.Location = new Point(645, 22);
            btnCreateCustomDat.Size = new Size(165, 30);
            btnCreateCustomDat.BackColor = Color.LightYellow;
            btnCreateCustomDat.Font = new Font(this.Font, FontStyle.Bold);
            btnCreateCustomDat.Click += new EventHandler(BtnCreateCustomDat_Click);

            grpCreate.Controls.Add(lblMsg);
            grpCreate.Controls.Add(txtCustomMessage);
            grpCreate.Controls.Add(btnCreateCustomDat);
            this.Controls.Add(grpCreate);

            // 3. 탭 컨트롤 레이아웃 (위치 조정)
            tabControl = new TabControl();
            tabControl.Location = new Point(15, 135);
            tabControl.Size = new Size(825, 480);

            tabBinary = new TabPage();
            tabBinary.Text = "[필터링] 알맹이 숫자 데이터만 추출";

            tabEncoding = new TabPage();
            tabEncoding.Text = "[교차대조] 모든 인코딩 완벽 한눈에 비교";

            tabControl.TabPages.Add(tabBinary);
            tabControl.TabPages.Add(tabEncoding);
            this.Controls.Add(tabControl);

            // [탭 1] 바이너리
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
            cmbByteGroup.SelectedIndex = 2;

            btnAnalyzeBinary = new Button();
            btnAnalyzeBinary.Text = "유령값 청소하고 데이터만 발려내기";
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

            // [탭 2] 인코딩
            btnAnalyzeEncoding = new Button();
            btnAnalyzeEncoding.Text = "국가별 전체 인코딩 실시간 동시 변환 실행";
            btnAnalyzeEncoding.Location = new Point(15, 15);
            btnAnalyzeEncoding.Size = new Size(320, 30);
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

        // [구현] 내가 입력한 텍스트 문장으로 바이너리 파일(.dat) 직접 생성하기
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
                    // 1. 입력받은 메시지를 바이트 배열로 변환
                    byte[] userTextBytes = Encoding.GetEncoding(949).GetBytes(txtCustomMessage.Text);

                    // 🔥 [핵심 난독화] 데이터를 가리 기 위한 암호화 키 (원하는 바이트 아무거나 가능)
                    byte xorKey = 0x5A; 

                    // 바이트 배열을 하나씩 돌면서 암호화 연산을 수행합니다.
                    // 이렇게 하면 데이터 값이 완전히 변해서 메모장이 복원을 못 합니다.
                    for (int i = 0; i < userTextBytes.Length; i++)
                    {
                        userTextBytes[i] = (byte)(userTextBytes[i] ^ xorKey);
                    }

                    // 암호화되어 완전히 깨진 바이트를 파일에 씁니다.
                    bw.Write(userTextBytes);

                    // 2. 뒤에는 기존처럼 고정 숫자 주입 (999) - 숫자도 숨기고 싶다면 똑같이 XOR 가능
                    for (int i = 0; i < 5; i++) bw.Write(999);
                    
                    // 3. 끝단 패딩용 0 주입
                    bw.Write((long)0);
                }

                MessageBox.Show("XOR 암호화가 적용된 'custom_test.dat' 파일이 생성되었습니다!\n\n이제 이 파일을 메모장으로 열어보세요. 완전히 깨져있을 겁니다.", "생성 성공");
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
                        MessageBox.Show("데이터 수집 완료: " + fileBytes.Length + " 바이트 로드됨", "성공");

                        btnAnalyzeBinary.Enabled = true;
                        btnAnalyzeEncoding.Enabled = true;
                    }
                    catch (Exception ex) { MessageBox.Show("파일 열기 실패: " + ex.Message); }
                }
            }
        }

        private void BtnAnalyzeBinary_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("======================================================================");
            sb.AppendLine(" [정밀 추출 결과] 앞쪽 깨진 문자열 및 의미없는 패딩 0을 제거한 유효 핵심 숫자 목록");
            sb.AppendLine("======================================================================");
            sb.AppendLine("데이터 주소\t|\t16진수 바이너리 코드\t|\t정제된 실제 데이터");
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

                // 유령 데이터 거르기 필터링 (텍스트 깨짐으로 인한 비정상 음수 및 패딩 0 필터)
                if (numericValue < 0 || numericValue == 0) continue;

                sb.AppendLine(i + "\t\t|\t" + hex.Trim() + "\t|\t" + numericValue);
                validCount++;
            }

            if (validCount == 0)
            {
                sb.AppendLine("\n[필터링 결과 없음] 유효한 데이터를 검출하지 못했습니다.");
                sb.AppendLine("바이트 선택 단위를 변경하거나 인코딩 교차대조 탭으로 이동하세요.");
            }

            txtBinaryResult.Text = sb.ToString();
        }

        private void BtnAnalyzeEncoding_Click(object sender, EventArgs e)
        {
            if (fileBytes == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("======================================================================");
            sb.AppendLine(" [5대 인코딩 완전 대조군 분석] 깨진 한자가 원래 문장으로 돌아오는 형식을 확인하세요.");
            sb.AppendLine("======================================================================");

            string[] encodingNames = { 
                "■ 1. UTF-8 (기본 유니코드 표준 인코딩)", 
                "■ 2. EUC-KR / CP949 (한국어 윈도우 완성형 표준 - 가장 유력)", 
                "■ 3. UTF-16 Little Endian (일반 유니코드 정규 포맷)", 
                "■ 4. UTF-16 Big Endian (서버/대형 시스템 통신 규격)", 
                "■ 5. ASCII (표준 영문 및 공통 제어 기호 영역)" 
            };

            List<Encoding> encs = new List<Encoding>();
            encs.Add(Encoding.UTF8);
            try { encs.Add(Encoding.GetEncoding(949)); } catch { encs.Add(Encoding.GetEncoding("EUC-KR")); }
            encs.Add(Encoding.Unicode);
            encs.Add(Encoding.BigEndianUnicode);
            encs.Add(Encoding.ASCII);

            for (int i = 0; i < encs.Count; i++)
            {
                sb.AppendLine("\n----------------------------------------------------------------------");
                sb.AppendLine(encodingNames[i]);
                sb.AppendLine("----------------------------------------------------------------------");
                
                string decoded = encs[i].GetString(fileBytes);
                string cleaned = decoded.Replace("\0", "").Trim();
                
                if (string.IsNullOrEmpty(cleaned))
                {
                    sb.AppendLine("[해당 형식으로 텍스트 복원 불가능]");
                }
                else
                {
                    sb.AppendLine(cleaned);
                }
            }

            txtEncodingResult.Text = sb.ToString();
        }
    }
}