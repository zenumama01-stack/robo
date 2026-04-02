import openai
# will default to `os.environ['OPENAI_API_KEY']` if not explicitly set
openai.api_key = "..."
# all client options can be configured just like the `OpenAI` instantiation counterpart
openai.base_url = "https://..."
openai.default_headers = {"x-foo": "true"}
# all API calls work in the exact same fashion as well
stream = openai.chat.completions.create(
    print(chunk.choices[0].delta.content or "", end="", flush=True)
