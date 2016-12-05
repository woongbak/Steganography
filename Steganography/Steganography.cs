
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

        public Steganography()
        {
            InitializeComponent();
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)imagePictureBox.Image; //원본 이미지를 비트맵 포맷으로 저장

            string text = dataTextBox.Text;  //이미지에 삽입할 문자열

            if (text.Equals(""))   //입력 텍스트가 공백인 경우
            {
                MessageBox.Show("The text you want to hide can't be empty", "Warning");

                return;
            }

            if (encryptCheckBox.Checked)     
            {
                if (passwordTextBox.Text.Length < 6)   //복호화에 사용할 키가 6자리 이하일 경우
                    {
                    MessageBox.Show("Please enter a password with at least 6 characters", "Warning");

                    return;
                }
                else
                {
                    text = Crypto.EncryptStringAES(text, passwordTextBox.Text); //이미지에 삽입할 텍스트, 사용자 입력 password를 Crypto() 메서드를 이용하여 암호화.
                }   // text에 암호화 결과가 저장된다.
            }

            bmp = SteganographyHelper.embedText(text, bmp); // 이미지에 텍스트 메시지를 삽입하는 메소드

            MessageBox.Show("Your text was hidden in the image successfully!", "Done");

            notesLabel.Text = "Notes: don't forget to save your new image.";
            notesLabel.ForeColor = Color.OrangeRed;
        }

        private void extractButton_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)imagePictureBox.Image;

            string extractedText = SteganographyHelper.extractText(bmp);

            if (encryptCheckBox.Checked)
            {
                try
                {
                    extractedText = Crypto.DecryptStringAES(extractedText, passwordTextBox.Text);   //추출된 text와 사용자 입력 password를 통해 평문 추출
                }
                catch
                {
                    MessageBox.Show("Wrong password", "Error");

                    return;
                }
            }

            dataTextBox.Text = extractedText;     // 추출된 평문을 DataTextBox에 텍스트 형식으로 출력
        }
        

          /// <summary>
          /// 사용자가 선택한 이미지를 imagePictureBox.image에 저장
          /// </summary>
          /// <param name="sender"></param>
          /// <param name="e"></param>
        private void imageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files (*.jpeg; *.png; *.bmp)|*.jpg; *.png; *.bmp";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                imagePictureBox.Image = Image.FromFile(open_dialog.FileName);
            }
        }

          /// <summary>
          /// 비트맵을 이미지 파일로 저장.
          /// output : Filename.Png, Filename.Bmp
          /// </summary>
          /// <param name="sender"></param>
          /// <param name="e"></param>
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

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Text Files|*.txt";

            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save_dialog.FileName, dataTextBox.Text);
            }
        }

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