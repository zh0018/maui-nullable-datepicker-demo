using System;
using Microsoft.Maui.Controls;

namespace NullableDatePickerDemo.Controls
{
    public class NullableDatePicker : ContentView
    {
        public static readonly BindableProperty SelectedDateProperty =
            BindableProperty.Create(
                nameof(SelectedDate),
                typeof(DateTime?),
                typeof(NullableDatePicker),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnSelectedDateChanged);

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public static readonly BindableProperty DateFormatProperty =
            BindableProperty.Create(
                nameof(DateFormat),
                typeof(string),
                typeof(NullableDatePicker),
                "d");

        public string DateFormat
        {
            get => (string)GetValue(DateFormatProperty);
            set => SetValue(DateFormatProperty, value);
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(NullableDatePicker),
                "请选择日期");

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        readonly DatePicker _picker;
        readonly Label _placeholderLabel;

        // 用于避免程序性设置触发 DateSelected
        bool _suppressDateSelected;
        // 标识当前是否由用户打开 picker（只有用户交互的 DateSelected 才写回 SelectedDate）
        bool _isUserOpeningPicker;

        public NullableDatePicker()
        {
            _picker = new DatePicker
            {
                Format = DateFormat,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            _placeholderLabel = new Label
            {
                Text = Placeholder,
                TextColor = Colors.Gray,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                Padding = new Thickness(6, 0),
                BackgroundColor = Colors.Transparent,
                InputTransparent = true // 让触摸事件穿透到下面的 DatePicker
            };

            // 把两个控件叠放（Label 在上，点击会穿透）
            var grid = new Grid();
            grid.Children.Add(_picker);
            grid.Children.Add(_placeholderLabel);

            Content = grid;

            // 事件
            _picker.Focused += (s, e) =>
            {
                _isUserOpeningPicker = true;

                // 打开前把底层日期设置为已选日期或今天，避免用户看到不合期望的起始日期
                _suppressDateSelected = true;
                _picker.Date = SelectedDate ?? DateTime.Today;
                _suppressDateSelected = false;
            };

            _picker.Unfocused += (s, e) =>
            {
                _isUserOpeningPicker = false;
                // 如果没有选择，显示占位文本（Label 覆盖）
                UpdatePlaceholderVisibility();
            };

            _picker.DateSelected += (s, e) =>
            {
                if (_suppressDateSelected)
                    return;

                if (!_isUserOpeningPicker)
                    return;

                // 用户确实选择了一个日期 -> 写回 SelectedDate
                SelectedDate = e.NewDate;
                _isUserOpeningPicker = false;
            };

            // 初始显示
            UpdatePlaceholderVisibility();
        }

        static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (NullableDatePicker)bindable;

            if (newValue is DateTime dt)
            {
                // 有值 -> 同步到底层 DatePicker（但要屏蔽触发）
                control._suppressDateSelected = true;
                control._picker.Date = dt;
                control._suppressDateSelected = false;

                // 确保格式为有效格式以正确显示
                control._picker.Format = control.DateFormat;
            }
            else
            {
                // 无值 -> 不把今天写回 SelectedDate，也不改底层 DatePicker 的 Date（避免触发）
                // 我们通过占位 Label 来“遮盖”显示，底层 Format 保持有效值
            }

            control.UpdatePlaceholderVisibility();
        }

        void UpdatePlaceholderVisibility()
        {
            _placeholderLabel.Text = Placeholder;
            _placeholderLabel.IsVisible = !SelectedDate.HasValue;
        }
    }
}
