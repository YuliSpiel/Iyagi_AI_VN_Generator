using System.Collections;
using System.IO;
using UnityEngine;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// Python rembg를 사용한 배경 제거
    /// </summary>
    public class BackgroundRemover : MonoBehaviour
    {
        private static string pythonPath = "python3"; // 시스템 Python 경로
        private static string rembgScriptPath; // rembg 스크립트 경로 (자동 생성)

        /// <summary>
        /// 이미지에서 배경 제거
        /// </summary>
        /// <param name="inputPath">원본 이미지 경로</param>
        /// <param name="outputPath">배경 제거된 이미지 저장 경로</param>
        /// <param name="onComplete">완료 콜백 (성공 여부)</param>
        public static IEnumerator RemoveBackground(string inputPath, string outputPath, System.Action<bool> onComplete)
        {
            // rembg 스크립트 생성 (없으면)
            if (string.IsNullOrEmpty(rembgScriptPath))
            {
                CreateRembgScript();
            }

            Debug.Log($"[BackgroundRemover] Starting background removal...");
            Debug.Log($"[BackgroundRemover] Input: {inputPath}");
            Debug.Log($"[BackgroundRemover] Output: {outputPath}");

            // 입력 파일 존재 확인
            if (!File.Exists(inputPath))
            {
                Debug.LogError($"[BackgroundRemover] Input file not found: {inputPath}");
                onComplete?.Invoke(false);
                yield break;
            }

            // 출력 디렉토리 생성
            string outputDir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Python 스크립트 실행
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{rembgScriptPath}\" \"{inputPath}\" \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            bool success = false;
            string output = "";
            string error = "";

            System.Diagnostics.Process process = null;
            System.Exception capturedException = null;

            // try-catch 안에서는 yield를 사용할 수 없으므로 프로세스 시작만 try-catch로 감싸기
            try
            {
                process = System.Diagnostics.Process.Start(processInfo);
            }
            catch (System.Exception e)
            {
                capturedException = e;
            }

            // 예외가 발생했으면 즉시 실패 처리
            if (capturedException != null)
            {
                Debug.LogError($"[BackgroundRemover] Failed to start process: {capturedException.Message}");
                Debug.LogError($"[BackgroundRemover] Stack trace: {capturedException.StackTrace}");
                onComplete?.Invoke(false);
                yield break;
            }

            if (process != null)
            {
                // 비동기로 출력 읽기
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                // 프로세스 완료 대기 (최대 30초)
                float timeout = 30f;
                float elapsed = 0f;

                while (!process.HasExited && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }

                if (!process.HasExited)
                {
                    Debug.LogError("[BackgroundRemover] Process timeout - killing process");
                    try
                    {
                        process.Kill();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[BackgroundRemover] Failed to kill process: {e.Message}");
                    }
                    success = false;
                }
                else
                {
                    int exitCode = process.ExitCode;
                    success = (exitCode == 0 && File.Exists(outputPath));

                    if (success)
                    {
                        Debug.Log($"[BackgroundRemover] Background removal successful!");
                    }
                    else
                    {
                        Debug.LogError($"[BackgroundRemover] Background removal failed with exit code: {exitCode}");
                        Debug.LogError($"[BackgroundRemover] Output: {output}");
                        Debug.LogError($"[BackgroundRemover] Error: {error}");
                    }
                }

                try
                {
                    process.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BackgroundRemover] Failed to close process: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("[BackgroundRemover] Failed to start Python process");
                success = false;
            }

            onComplete?.Invoke(success);
        }

        /// <summary>
        /// rembg Python 스크립트 생성
        /// </summary>
        private static void CreateRembgScript()
        {
            string scriptDir = Path.Combine(Application.dataPath, "Python");
            if (!Directory.Exists(scriptDir))
            {
                Directory.CreateDirectory(scriptDir);
            }

            rembgScriptPath = Path.Combine(scriptDir, "remove_bg.py");

            string scriptContent = @"#!/usr/bin/env python3
import sys
import os

try:
    from rembg import remove
    from PIL import Image
except ImportError as e:
    print(f'Error: Required package not installed: {e}', file=sys.stderr)
    print('Please install: pip install rembg pillow', file=sys.stderr)
    sys.exit(1)

def remove_background(input_path, output_path):
    try:
        # 입력 이미지 열기
        with open(input_path, 'rb') as input_file:
            input_data = input_file.read()

        # 배경 제거
        output_data = remove(input_data)

        # 결과 저장
        with open(output_path, 'wb') as output_file:
            output_file.write(output_data)

        print(f'Background removed successfully: {output_path}')
        return True

    except Exception as e:
        print(f'Error removing background: {e}', file=sys.stderr)
        import traceback
        traceback.print_exc()
        return False

if __name__ == '__main__':
    if len(sys.argv) != 3:
        print('Usage: python remove_bg.py <input_path> <output_path>', file=sys.stderr)
        sys.exit(1)

    input_path = sys.argv[1]
    output_path = sys.argv[2]

    if not os.path.exists(input_path):
        print(f'Error: Input file not found: {input_path}', file=sys.stderr)
        sys.exit(1)

    success = remove_background(input_path, output_path)
    sys.exit(0 if success else 1)
";

            File.WriteAllText(rembgScriptPath, scriptContent);
            Debug.Log($"[BackgroundRemover] Created rembg script at: {rembgScriptPath}");

            // macOS/Linux에서 실행 권한 부여
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor)
            {
                try
                {
                    var chmodProcess = System.Diagnostics.Process.Start("chmod", $"+x \"{rembgScriptPath}\"");
                    chmodProcess?.WaitForExit();
                    chmodProcess?.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BackgroundRemover] Could not set execute permission: {e.Message}");
                }
            }
        }

        /// <summary>
        /// rembg 설치 확인 및 안내
        /// </summary>
        public static void CheckRembgInstallation()
        {
            Debug.Log("[BackgroundRemover] Checking rembg installation...");

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = "-c \"import rembg; print('rembg installed')\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Debug.Log("[BackgroundRemover] ✅ rembg is installed");
                    }
                    else
                    {
                        Debug.LogWarning("[BackgroundRemover] ⚠️ rembg is not installed");
                        Debug.LogWarning("[BackgroundRemover] Please run: pip install rembg[gpu] pillow");
                        Debug.LogWarning("[BackgroundRemover] Or without GPU: pip install rembg pillow");
                    }

                    process.Close();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BackgroundRemover] Failed to check installation: {e.Message}");
            }
        }

        /// <summary>
        /// Python 경로 설정
        /// </summary>
        public static void SetPythonPath(string path)
        {
            pythonPath = path;
            Debug.Log($"[BackgroundRemover] Python path set to: {pythonPath}");
        }
    }
}
