import socket
import json
import sys
from io import StringIO
import struct
import time

HOST = '127.0.0.1'
PORT = 5555

def execute_code(code):
    """Execute Python code and capture output/errors"""
    old_stdout = sys.stdout
    old_stderr = sys.stderr
    
    redirected_output = StringIO()
    redirected_error = StringIO()
    
    sys.stdout = redirected_output
    sys.stderr = redirected_error
    
    try:
        exec(code)
        output = redirected_output.getvalue()
        errors = redirected_error.getvalue()
    except Exception as e:
        output = redirected_output.getvalue()
        errors = redirected_error.getvalue() + str(e)
    finally:
        sys.stdout = old_stdout
        sys.stderr = old_stderr
    
    return output, errors

def main():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        server_socket.bind((HOST, PORT))
        server_socket.listen()
        
        print(f"Python worker listening on {HOST}:{PORT}", flush=True)
        
        while True:
            try:
                conn, addr = server_socket.accept()
                print(f"Connection from {addr}", flush=True)
                
                # DON'T use 'with conn:' - it closes too early!
                # Read code length (4 bytes, big-endian)
                length_data = conn.recv(4)
                if len(length_data) < 4:
                    print("Invalid length data received", flush=True)
                    conn.close()
                    continue
                
                code_length = struct.unpack('>I', length_data)[0]
                print(f"Expecting {code_length} bytes of code", flush=True)
                
                # Read exact code
                code = b''
                while len(code) < code_length:
                    chunk = conn.recv(min(4096, code_length - len(code)))
                    if not chunk:
                        break
                    code += chunk
                
                code_str = code.decode('utf-8')
                print(f"Received code:\n{code_str}", flush=True)
                
                # Execute
                output, errors = execute_code(code_str)
                print(f"Execution complete. Output: '{output}', Errors: '{errors}'", flush=True)
                
                # Send response
                response = json.dumps({'output': output, 'errors': errors})
                response_bytes = response.encode('utf-8')
                
                print(f"Sending response: {response}", flush=True)
                
                # Send response length + response
                conn.sendall(struct.pack('>I', len(response_bytes)))
                conn.sendall(response_bytes)
                
                print(f"Response sent ({len(response_bytes)} bytes)", flush=True)
                
                # Wait a bit to ensure data is transmitted
                time.sleep(0.1)
                
                # NOW close the connection
                conn.close()
                print("Connection closed", flush=True)
                    
            except Exception as e:
                print(f"Error handling connection: {e}", flush=True)
                try:
                    error_response = json.dumps({'output': '', 'errors': str(e)})
                    error_bytes = error_response.encode('utf-8')
                    conn.sendall(struct.pack('>I', len(error_bytes)))
                    conn.sendall(error_bytes)
                    time.sleep(0.1)
                    conn.close()
                except:
                    pass

if __name__ == '__main__':
    main()