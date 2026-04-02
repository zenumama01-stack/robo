import re
DEFINE_EXTRACT_REGEX = re.compile(r'^ *# *define (\w*)', re.MULTILINE)
def main(out_dir, headers):
  defines = []
  for filename in headers:
    with open(filename, 'r') as f:
      content = f.read()
      defines += read_defines(content)
  push_and_undef = ''
  for define in defines:
    push_and_undef += '#pragma push_macro("%s")\n' % define
    push_and_undef += '#undef %s\n' % define
  with open(os.path.join(out_dir, 'push_and_undef_node_defines.h'), 'w') as o:
    o.write(push_and_undef)
  pop = ''
    pop += '#pragma pop_macro("%s")\n' % define
  with open(os.path.join(out_dir, 'pop_node_defines.h'), 'w') as o:
    o.write(pop)
def read_defines(content):
  for match in DEFINE_EXTRACT_REGEX.finditer(content):
    defines.append(match.group(1))
  return defines
  main(sys.argv[1], sys.argv[2:])
