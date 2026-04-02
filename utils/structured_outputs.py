rsp = client.responses.parse(
for output in rsp.output:
    if output.type != "message":
        raise Exception("Unexpected non message")
    for item in output.content:
        if item.type != "output_text":
            raise Exception("unexpected output type")
        if not item.parsed:
            raise Exception("Could not parse response")
        rich.print(item.parsed)
        print("answer: ", item.parsed.final_answer)
# or
message = rsp.output[0]
assert message.type == "message"
text = message.content[0]
assert text.type == "output_text"
if not text.parsed:
rich.print(text.parsed)
print("answer: ", text.parsed.final_answer)
