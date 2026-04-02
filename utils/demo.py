client = OpenAI()
# Non-streaming:
print("----- standard request -----")
    model="gpt-4",
            "content": "Say this is a test",
print(completion.choices[0].message.content)
# Streaming:
print("----- streaming request -----")
stream = client.chat.completions.create(
for chunk in stream:
    if not chunk.choices:
    print(chunk.choices[0].delta.content, end="")
# Response headers:
print("----- custom response headers test -----")
response = client.chat.completions.with_raw_response.create(
completion = response.parse()
print(response.request_id)
