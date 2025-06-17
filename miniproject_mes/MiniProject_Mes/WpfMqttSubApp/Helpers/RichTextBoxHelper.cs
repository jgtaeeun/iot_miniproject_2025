using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace WpfMqttSubApp.Helpers
{
    public static class RichTextBoxHelper
    {
        // 사용자가 만든 바인딩할 문자열 프로퍼티  BindableDocument
        public static readonly DependencyProperty BindableDocumentProperty =
            DependencyProperty.RegisterAttached(
                "BindableDocument",
                typeof(string),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableDocumentChanged));

        // 속성 BindableDocument의 게터함수
        public static string GetBindableDocument(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableDocumentProperty);
        }

        // 속성 BindableDocument의 세터함수
        public static void SetBindableDocument(DependencyObject obj, string value)
        {
            obj.SetValue(BindableDocumentProperty, value);
        }

        // 속성값 변경되었을 때 이벤트처리
        private static void OnBindableDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox richTextBox)
            {
                // 기존 문서 클리어
                richTextBox.Document.Blocks.Clear();
                // 새 문자열을 포함하는 Paragraph 추가
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(e.NewValue as string ?? string.Empty)));
            }
        }
    }
}
