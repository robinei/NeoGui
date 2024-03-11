#!/usr/bin/env python

import os
import argparse
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler


parser = argparse.ArgumentParser(prog='Development server', description='Serves the WASM program')
parser.add_argument('-r', '--release', action='store_true', help='Serve release config')
args = parser.parse_args()

CONFIG = 'Release' if args.release else 'Debug'
ROOT = os.path.abspath(os.path.dirname(__file__))
WWWROOT = os.path.join(ROOT, 'bin', CONFIG, 'net8.0', 'wwwroot')


class Handler(SimpleHTTPRequestHandler):
    """This handler uses server.base_path instead of always using os.getcwd()"""
    def translate_path(self, path):
        path = '.' + path
        if path == './index.html' or path == './':
            return os.path.join(ROOT,  'index.html')
        return os.path.join(WWWROOT, path)


print('Serving on http://localhost:3000')
ThreadingHTTPServer(('', 3000), Handler).serve_forever()
