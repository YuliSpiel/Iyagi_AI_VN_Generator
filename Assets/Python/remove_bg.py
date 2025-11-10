#!/usr/bin/env python3
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
