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

        public Steganography()  //각종 component 요소 초기화 진행(이벤트핸들러 등)
        {
            InitializeComponent();
        }

        private void hideButton_Click(object sender, EventArgs e)  // hide 버튼 클릭할 때 event 발생
        {
            bmp = (Bitmap)imagePictureBox.Image;

            string text = dataTextBox.Text;       // 숨길 문자 

            if (text.Equals(""))    // 숨길 문자가 없는 경우
            {
                MessageBox.Show("The text you want to hide can't be empty", "Warning");

                return;
            }

            if (encryptCheckBox.Checked)  // 암호화에 체크한 경우
            {
                if (passwordTextBox.Text.Length < 6)  //패스워드가 6글자 미만인 경우
                {
                    MessageBox.Show("Please enter a password with at least 6 characters", "Warning");

                    return;
                }
                else        // 정상적으로 AES 암호화 수행
                {
                    text = Crypto.EncryptStringAES(text, passwordTextBox.Text);  //패스워드와 숨길 데이터를 입력 값으로 받음.
                }
            }

            bmp = SteganographyHelper.embedText(text, bmp);    //이미지 안에 데이터를 숨기는 역할 수행, embedText()메소드 호출

            MessageBox.Show("Your text was hidden in the image successfully!", "Done");  

            notesLabel.Text = "Notes: don't forget to save your new image.";    
            notesLabel.ForeColor = Color.OrangeRed;
        }

        private void extractButton_Click(object sender, EventArgs e)    // 데이터 추출 버튼을 누를경우 수행
        {
            bmp = (Bitmap)imagePictureBox.Image;                // * open한 이미지를 가져옴, open하지 않은 경우에 대한 처리가 없는걸 볼 수 있다..

            string extractedText = SteganographyHelper.extractText(bmp);   

            if (encryptCheckBox.Checked)   //암호화에 체크한 경우 입력된 패스워드 값을 토대로 Decrypt함
            {
                try
                {
                    extractedText = Crypto.DecryptStringAES(extractedText, passwordTextBox.Text);
                }
                catch // 패스워드가 틀릴 경우 "Wrong password" 출력
                {
                    MessageBox.Show("Wrong password", "Error");

                    return;
                }
            }

            dataTextBox.Text = extractedText;
        }

        private void imageToolStripMenuItem1_Click(object sender, EventArgs e)  //새로운 이미지를 open하는 경우
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files (*.jpeg; *.png; *.bmp)|*.jpg; *.png; *.bmp";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                imagePictureBox.Image = Image.FromFile(open_dialog.FileName);
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)  // 변경한 이미지를 저장하는 경우
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

        private void textToolStripMenuItem_Click(object sender, EventArgs e)  // 텍스트 파일을 저장하는 경우
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Text Files|*.txt";

            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save_dialog.FileName, dataTextBox.Text);
            }
        }

        private void textToolStripMenuItem1_Click(object sender, EventArgs e) // 텍스트 파일을 open하는 경우
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Text Files|*.txt";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                dataTextBox.Text = File.ReadAllText(open_dialog.FileName);
            }
        }

        private void imagePictureBox_Click(object sender, EventArgs e)
        {

        }

        private void Steganography_Load(object sender, EventArgs e)
        {

        }

    }
}