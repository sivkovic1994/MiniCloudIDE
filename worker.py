import socket
import io
import json
from contextlib import redirect_stdout, redirect_stderr

HOST = '127.0.0.1'
PORT = 5555


def recv_all(conn, length):
    """Receive exact number of bytes"""
    data = b''
    while len(data) < length:
        packet = conn.recv(length - len(data))
        if not packet:
            return None
        data += packet
    return data


def execute_code(code):
    """Execute Python code and capture output/errors"""
    stdout = io.StringIO()
    stderr = io.StringIO()

    try:
        with redirect_stdout(stdout), redirect_stderr(stderr):
            exec(code, {})

        return {
            "output": stdout.getvalue(),
            "errors": stderr.getvalue()
        }

    except Exception as e:
        return {
            "output": stdout.getvalue(),
            "errors": f"{stderr.getvalue()}{type(e).__name__}: {str(e)}"
        }


def handle_client(conn):
    try:
        # receive length (4 bytes)
        length_data = recv_all(conn, 4)
        if not length_data:
            return

        code_length = int.from_bytes(length_data, "big")

        # receive actual code
        code_data = recv_all(conn, code_length)
        if not code_data:
            return

        code = code_data.decode("utf-8")

        # execute code
        result = execute_code(code)

        # send response
        response = json.dumps(result).encode("utf-8")

        conn.sendall(len(response).to_bytes(4, "big"))
        conn.sendall(response)

    except Exception as e:
        error = json.dumps({
            "output": "",
            "errors": f"Worker error: {str(e)}"
        }).encode("utf-8")

        conn.sendall(len(error).to_bytes(4, "big"))
        conn.sendall(error)


def main():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server:
        server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        server.bind((HOST, PORT))
        server.listen()

        print(f"Python worker listening on {HOST}:{PORT}")

        while True:
            conn, addr = server.accept()
            with conn:
                handle_client(conn)


if __name__ == "__main__":
    main()