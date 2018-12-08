using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

namespace Steganography
{
    public partial class Steganography : Form
    {
        private Bitmap bmp = null;
        private string extractedText = string.Empty;

        // 생성자를 만들고 초기화
        public Steganography()
        {
            InitializeComponent();
        }
        
        // Hide 버튼을 누르면 발생하는 이벤트 생성 함수
        private void hideButton_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)imagePictureBox.Image;

            string text = dataTextBox.Text;

            // 사용자가 입력값을 넣지 않으면 경고창을 띄워줌
            if (text.Equals(""))
            {
                MessageBox.Show("The text you want to hide can't be empty", "Warning");

                return;
            }

            // encrypted CheckBox가 체크되어있으면 
            if (encryptCheckBox.Checked)
            {
                // 입력된 키 길이가 6글자보다 짧으면
                if (passwordTextBox.Text.Length < 6)
                {
                    // 6글자를 넘기라는 경고창을 띄워줌
                    MessageBox.Show("Please enter a password with at least 6 characters", "Warning");

                    return;
                }
                // 입력된 키 길이가 6글자 이상이면 입력된 키를 AES로 암호화
                else
                {
                    text = Crypto.EncryptStringAES(text, passwordTextBox.Text);
                }
            }

            // 이미지에 사용자가 입력한 텍스트를 숨기는 함수 적용
            bmp = SteganographyHelper.embedText(text, bmp);

            MessageBox.Show("Your text was hidden in the image successfully!", "Done");
       
            notesLabel.Text = "Notes: don't forget to save your new image.";
            notesLabel.ForeColor = Color.OrangeRed;
        }

        // Extract 버튼을 누르면 발생하는 이벤트 생성 함수
        private void extractButton_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)imagePictureBox.Image;

            // 이미지에서 추출된 텍스트 저장하는 변수 선언
            string extractedText = SteganographyHelper.extractText(bmp);

            // encrypted CheckBox가 체크되어있으면
            if (encryptCheckBox.Checked)
            {
                // 입력한 키를 가지고 추출된 텍스트의 AES 복호화 진행
                try
                {
                    extractedText = Crypto.DecryptStringAES(extractedText, passwordTextBox.Text);
                }
                // 키가 다르면 에러메시지 출력
                catch
                {
                    MessageBox.Show("Wrong password", "Error");

                    return;
                }
            }

            dataTextBox.Text = extractedText;
        }

        // 프로그램에 이미지를 OPEN하는 함수 
        private void imageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            // JPEG, PNG, BMP 형식의 이미지 파일만 필터링
            open_dialog.Filter = "Image Files (*.jpeg; *.png; *.bmp)|*.jpg; *.png; *.bmp";

            // 형식에 맞는 이미지가 들어오면 해당 이미지를 이미지박스에 출력
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                imagePictureBox.Image = Image.FromFile(open_dialog.FileName);
            }
        }

        // Hide된 이미지를 저장하는 MenuItem 클릭 이벤트 생성 함수
        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Png Image|*.png|Bitmap Image|*.bmp";
       
            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                switch (save_dialog.FilterIndex)
                {
                    case 0:
                        {
                            bmp.Save(save_dialog.FileName, ImageFormat.Png);
                        }break;
                    case 1:
                        {
                            bmp.Save(save_dialog.FileName, ImageFormat.Bmp);
                        } break;
                }

                notesLabel.Text = "Notes:";
                notesLabel.ForeColor = Color.Black;
            }
        }

        // 추출된 텍스트를 저장하는 MenuItem 클릭 이벤트 생성 함수
        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Text Files|*.txt";

            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save_dialog.FileName, dataTextBox.Text);
            }
        }

        // 외부에서 이미지를 불러오는 MenuItem 클릭 이벤트 생성 
        private void textToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Text Files|*.txt";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                dataTextBox.Text = File.ReadAllText(open_dialog.FileName);
            }
        }

    }
}
