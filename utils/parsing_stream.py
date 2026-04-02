with client.chat.completions.stream(
) as stream:
        if event.type == "content.delta":
            print(event.delta, end="", flush=True)
        elif event.type == "content.done":
            print("\n")
            if event.parsed is not None:
                print(f"answer: {event.parsed.final_answer}")
        elif event.type == "refusal.delta":
        elif event.type == "refusal.done":
print("---------------")
rich.print(stream.get_final_completion())
