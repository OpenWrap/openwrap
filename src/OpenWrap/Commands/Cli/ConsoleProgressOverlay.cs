using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class ConsoleProgressOverlay
    {
        Dictionary<string, Overlay> _progressPositions = new Dictionary<string, Overlay>();


        public void WriteProgress(string id, string text, int progress)
        {
            if (progress < 0)
                throw new ArgumentOutOfRangeException("progress");
            if (progress > 200)
                Finished(id);
            else
                Update(id, text, progress);
        }

        void Update(string id, string text, int progress)
        {
            Overlay overlay;
            if (!_progressPositions.TryGetValue(id, out overlay))
                _progressPositions[id] = overlay = new Overlay(_progressPositions.Count, text);
            overlay.SetProgress(progress);
        }

        void Finished(string id)
        {
            Overlay overlay;
            if (_progressPositions.TryGetValue(id, out overlay) == false) return;

            var overlays = _progressPositions.Values
                .OrderBy(_=>_)
                .SkipWhile(_ => _ != overlay)
                .ToList();
            if (overlays.Count == 1)
                overlays[0].Remove();
            else
                overlays.ForEach(_ => _.MoveUp());
            _progressPositions.Remove(id);
            overlay.Dispose();
        }

        class Overlay : IComparable<Overlay>
        {
            int _overlayLineTopInBuffer;
            int _cachedLineTopInBuffer;

            public Overlay(int top, string text)
            {
                _overlayLineTopInBuffer = Console.WindowTop + top;

                Console.BufferHeight++;
                _cachedLineTopInBuffer = (Console.BufferHeight - 1);

                // copy removed bits to empty line at top of buffer
                Console.MoveBufferArea(0, _overlayLineTopInBuffer, Console.WindowWidth, 1, 0, _cachedLineTopInBuffer);
                Write(text);
            }

            public void Write(string text)
            {
                var rightMargin = Console.WindowWidth - 10;
                if (text.Length > rightMargin - 1) text = text.Substring(0, rightMargin - 1);
                if (text.Length <= rightMargin) text += new string(' ', rightMargin - text.Length);
                using (ConsoleEx.Position(0, _overlayLineTopInBuffer))
                using (ConsoleEx.Color(ConsoleEx.ProgressTextForeground, ConsoleEx.ProgressTextBackground))
                    Console.Write(text);
            }

            public void MoveUp()
            {
                var stillVisible = _overlayLineTopInBuffer > Console.WindowTop;

                if (stillVisible)
                    Console.MoveBufferArea(0, _overlayLineTopInBuffer, Console.WindowWidth, 1, 0, _overlayLineTopInBuffer - 1);
                if (_cachedLineTopInBuffer == Console.BufferHeight - 1)
                {
                    RestoreText();
                    Console.BufferHeight--;
                }

                _overlayLineTopInBuffer--;
                _cachedLineTopInBuffer--;
            }
            public void Dispose()
            {
            }

            void RestoreText()
            {
                Console.MoveBufferArea(0, _cachedLineTopInBuffer, Console.WindowWidth, 1, 0, _overlayLineTopInBuffer);
            }

            public void SetProgress(int progress)
            {
                using (ConsoleEx.Position(Console.WindowWidth - 10, _overlayLineTopInBuffer))
                using (ConsoleEx.Color(ConsoleEx.ProgressBarForeground, ConsoleEx.ProgressBarBackground))
                {
                    var fc = Console.ForegroundColor;
                    var bc = Console.BackgroundColor;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.BackgroundColor = ConsoleColor.Black;

                    Console.Write(progress.ToProgressBar());
                    Console.ForegroundColor = fc;
                    Console.BackgroundColor = bc;
                }
            }

            public void Remove()
            {
                RestoreText();
                Console.BufferHeight--;
            }

            public int CompareTo(Overlay other)
            {
                if (other == null) return 1;          
                return this._overlayLineTopInBuffer.CompareTo(other._overlayLineTopInBuffer);
            }
        }
    }
}