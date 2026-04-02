# pyright: basic
from typing import Any, TypeVar, Callable, Optional, NamedTuple
from typing_extensions import TypeAlias
from .._extras import pandas as pd
class Remediation(NamedTuple):
    immediate_msg: Optional[str] = None
    necessary_msg: Optional[str] = None
    necessary_fn: Optional[Callable[[Any], Any]] = None
    optional_msg: Optional[str] = None
    optional_fn: Optional[Callable[[Any], Any]] = None
    error_msg: Optional[str] = None
OptionalDataFrameT = TypeVar("OptionalDataFrameT", bound="Optional[pd.DataFrame]")
def num_examples_validator(df: pd.DataFrame) -> Remediation:
    This validator will only print out the number of examples and recommend to the user to increase the number of examples if less than 100.
    MIN_EXAMPLES = 100
    optional_suggestion = (
        ""
        if len(df) >= MIN_EXAMPLES
        else ". In general, we recommend having at least a few hundred examples. We've found that performance tends to linearly increase for every doubling of the number of examples"
    immediate_msg = f"\n- Your file contains {len(df)} prompt-completion pairs{optional_suggestion}"
    return Remediation(name="num_examples", immediate_msg=immediate_msg)
def necessary_column_validator(df: pd.DataFrame, necessary_column: str) -> Remediation:
    This validator will ensure that the necessary column is present in the dataframe.
    def lower_case_column(df: pd.DataFrame, column: Any) -> pd.DataFrame:
        cols = [c for c in df.columns if str(c).lower() == column]
        df.rename(columns={cols[0]: column.lower()}, inplace=True)
        return df
    immediate_msg = None
    necessary_fn = None
    necessary_msg = None
    error_msg = None
    if necessary_column not in df.columns:
        if necessary_column in [str(c).lower() for c in df.columns]:
            def lower_case_column_creator(df: pd.DataFrame) -> pd.DataFrame:
                return lower_case_column(df, necessary_column)
            necessary_fn = lower_case_column_creator
            immediate_msg = f"\n- The `{necessary_column}` column/key should be lowercase"
            necessary_msg = f"Lower case column name to `{necessary_column}`"
            error_msg = f"`{necessary_column}` column/key is missing. Please make sure you name your columns/keys appropriately, then retry"
    return Remediation(
        name="necessary_column",
        immediate_msg=immediate_msg,
        necessary_msg=necessary_msg,
        necessary_fn=necessary_fn,
        error_msg=error_msg,
def additional_column_validator(df: pd.DataFrame, fields: list[str] = ["prompt", "completion"]) -> Remediation:
    This validator will remove additional columns from the dataframe.
    additional_columns = []
    necessary_fn = None  # type: ignore
    if len(df.columns) > 2:
        additional_columns = [c for c in df.columns if c not in fields]
        warn_message = ""
        for ac in additional_columns:
            dups = [c for c in additional_columns if ac in c]
            if len(dups) > 0:
                warn_message += f"\n  WARNING: Some of the additional columns/keys contain `{ac}` in their name. These will be ignored, and the column/key `{ac}` will be used instead. This could also result from a duplicate column/key in the provided file."
        immediate_msg = f"\n- The input file should contain exactly two columns/keys per row. Additional columns/keys present are: {additional_columns}{warn_message}"
        necessary_msg = f"Remove additional columns/keys: {additional_columns}"
        def necessary_fn(x: Any) -> Any:
            return x[fields]
        name="additional_column",
def non_empty_field_validator(df: pd.DataFrame, field: str = "completion") -> Remediation:
    This validator will ensure that no completion is empty.
    if df[field].apply(lambda x: x == "").any() or df[field].isnull().any():
        empty_rows = (df[field] == "") | (df[field].isnull())
        empty_indexes = df.reset_index().index[empty_rows].tolist()
        immediate_msg = f"\n- `{field}` column/key should not contain empty strings. These are rows: {empty_indexes}"
            return x[x[field] != ""].dropna(subset=[field])
        necessary_msg = f"Remove {len(empty_indexes)} rows with empty {field}s"
        name=f"empty_{field}",
def duplicated_rows_validator(df: pd.DataFrame, fields: list[str] = ["prompt", "completion"]) -> Remediation:
    This validator will suggest to the user to remove duplicate rows if they exist.
    duplicated_rows = df.duplicated(subset=fields)
    duplicated_indexes = df.reset_index().index[duplicated_rows].tolist()
    optional_msg = None
    optional_fn = None  # type: ignore
    if len(duplicated_indexes) > 0:
        immediate_msg = f"\n- There are {len(duplicated_indexes)} duplicated {'-'.join(fields)} sets. These are rows: {duplicated_indexes}"
        optional_msg = f"Remove {len(duplicated_indexes)} duplicate rows"
        def optional_fn(x: Any) -> Any:
            return x.drop_duplicates(subset=fields)
        name="duplicated_rows",
        optional_msg=optional_msg,
        optional_fn=optional_fn,
def long_examples_validator(df: pd.DataFrame) -> Remediation:
    This validator will suggest to the user to remove examples that are too long.
    ft_type = infer_task_type(df)
    if ft_type != "open-ended generation":
        def get_long_indexes(d: pd.DataFrame) -> Any:
            long_examples = d.apply(lambda x: len(x.prompt) + len(x.completion) > 10000, axis=1)
            return d.reset_index().index[long_examples].tolist()
        long_indexes = get_long_indexes(df)
        if len(long_indexes) > 0:
            immediate_msg = f"\n- There are {len(long_indexes)} examples that are very long. These are rows: {long_indexes}\nFor conditional generation, and for classification the examples shouldn't be longer than 2048 tokens."
            optional_msg = f"Remove {len(long_indexes)} long examples"
                long_indexes_to_drop = get_long_indexes(x)
                if long_indexes != long_indexes_to_drop:
                        f"The indices of the long examples has changed as a result of a previously applied recommendation.\nThe {len(long_indexes_to_drop)} long examples to be dropped are now at the following indices: {long_indexes_to_drop}\n"
                return x.drop(long_indexes_to_drop)
        name="long_examples",
def common_prompt_suffix_validator(df: pd.DataFrame) -> Remediation:
    This validator will suggest to add a common suffix to the prompt if one doesn't already exist in case of classification or conditional generation.
    # Find a suffix which is not contained within the prompt otherwise
    suggested_suffix = "\n\n### =>\n\n"
    suffix_options = [
        " ->",
        "\n\n###\n\n",
        "\n\n===\n\n",
        "\n\n---\n\n",
        "\n\n===>\n\n",
        "\n\n--->\n\n",
    for suffix_option in suffix_options:
        if suffix_option == " ->":
            if df.prompt.str.contains("\n").any():
        if df.prompt.str.contains(suffix_option, regex=False).any():
        suggested_suffix = suffix_option
    display_suggested_suffix = suggested_suffix.replace("\n", "\\n")
    if ft_type == "open-ended generation":
        return Remediation(name="common_suffix")
    def add_suffix(x: Any, suffix: Any) -> Any:
        x["prompt"] += suffix
        return x
    common_suffix = get_common_xfix(df.prompt, xfix="suffix")
    if (df.prompt == common_suffix).all():
        error_msg = f"All prompts are identical: `{common_suffix}`\nConsider leaving the prompts blank if you want to do open-ended generation, otherwise ensure prompts are different"
        return Remediation(name="common_suffix", error_msg=error_msg)
    if common_suffix != "":
        common_suffix_new_line_handled = common_suffix.replace("\n", "\\n")
        immediate_msg = f"\n- All prompts end with suffix `{common_suffix_new_line_handled}`"
        if len(common_suffix) > 10:
            immediate_msg += f". This suffix seems very long. Consider replacing with a shorter suffix, such as `{display_suggested_suffix}`"
        if df.prompt.str[: -len(common_suffix)].str.contains(common_suffix, regex=False).any():
            immediate_msg += f"\n  WARNING: Some of your prompts contain the suffix `{common_suffix}` more than once. We strongly suggest that you review your prompts and add a unique suffix"
        immediate_msg = "\n- Your data does not contain a common separator at the end of your prompts. Having a separator string appended to the end of the prompt makes it clearer to the fine-tuned model where the completion should begin. See https://platform.openai.com/docs/guides/fine-tuning/preparing-your-dataset for more detail and examples. If you intend to do open-ended generation, then you should leave the prompts empty"
    if common_suffix == "":
        optional_msg = f"Add a suffix separator `{display_suggested_suffix}` to all prompts"
            return add_suffix(x, suggested_suffix)
        name="common_completion_suffix",
def common_prompt_prefix_validator(df: pd.DataFrame) -> Remediation:
    This validator will suggest to remove a common prefix from the prompt if a long one exist.
    MAX_PREFIX_LEN = 12
    common_prefix = get_common_xfix(df.prompt, xfix="prefix")
    if common_prefix == "":
        return Remediation(name="common_prefix")
    def remove_common_prefix(x: Any, prefix: Any) -> Any:
        x["prompt"] = x["prompt"].str[len(prefix) :]
    if (df.prompt == common_prefix).all():
        # already handled by common_suffix_validator
    if common_prefix != "":
        immediate_msg = f"\n- All prompts start with prefix `{common_prefix}`"
        if MAX_PREFIX_LEN < len(common_prefix):
            immediate_msg += ". Fine-tuning doesn't require the instruction specifying the task, or a few-shot example scenario. Most of the time you should only add the input data into the prompt, and the desired output into the completion"
            optional_msg = f"Remove prefix `{common_prefix}` from all prompts"
                return remove_common_prefix(x, common_prefix)
        name="common_prompt_prefix",
def common_completion_prefix_validator(df: pd.DataFrame) -> Remediation:
    This validator will suggest to remove a common prefix from the completion if a long one exist.
    MAX_PREFIX_LEN = 5
    common_prefix = get_common_xfix(df.completion, xfix="prefix")
    ws_prefix = len(common_prefix) > 0 and common_prefix[0] == " "
    if len(common_prefix) < MAX_PREFIX_LEN:
    def remove_common_prefix(x: Any, prefix: Any, ws_prefix: Any) -> Any:
        x["completion"] = x["completion"].str[len(prefix) :]
        if ws_prefix:
            # keep the single whitespace as prefix
            x["completion"] = f" {x['completion']}"
    if (df.completion == common_prefix).all():
    immediate_msg = f"\n- All completions start with prefix `{common_prefix}`. Most of the time you should only add the output data into the completion, without any prefix"
    optional_msg = f"Remove prefix `{common_prefix}` from all completions"
        return remove_common_prefix(x, common_prefix, ws_prefix)
        name="common_completion_prefix",
def common_completion_suffix_validator(df: pd.DataFrame) -> Remediation:
    This validator will suggest to add a common suffix to the completion if one doesn't already exist in case of classification or conditional generation.
    if ft_type == "open-ended generation" or ft_type == "classification":
    common_suffix = get_common_xfix(df.completion, xfix="suffix")
    if (df.completion == common_suffix).all():
        error_msg = f"All completions are identical: `{common_suffix}`\nEnsure completions are different, otherwise the model will just repeat `{common_suffix}`"
    # Find a suffix which is not contained within the completion otherwise
    suggested_suffix = " [END]"
        " END",
        "***",
        "+++",
        "&&&",
        "$$$",
        "@@@",
        "%%%",
        if df.completion.str.contains(suffix_option, regex=False).any():
        x["completion"] += suffix
        immediate_msg = f"\n- All completions end with suffix `{common_suffix_new_line_handled}`"
        if df.completion.str[: -len(common_suffix)].str.contains(common_suffix, regex=False).any():
            immediate_msg += f"\n  WARNING: Some of your completions contain the suffix `{common_suffix}` more than once. We suggest that you review your completions and add a unique ending"
        immediate_msg = "\n- Your data does not contain a common ending at the end of your completions. Having a common ending string appended to the end of the completion makes it clearer to the fine-tuned model where the completion should end. See https://platform.openai.com/docs/guides/fine-tuning/preparing-your-dataset for more detail and examples."
        optional_msg = f"Add a suffix ending `{display_suggested_suffix}` to all completions"
def completions_space_start_validator(df: pd.DataFrame) -> Remediation:
    This validator will suggest to add a space at the start of the completion if it doesn't already exist. This helps with tokenization.
    def add_space_start(x: Any) -> Any:
        x["completion"] = x["completion"].apply(lambda s: ("" if s.startswith(" ") else " ") + s)
    optional_fn = None
    if df.completion.str[:1].nunique() != 1 or df.completion.values[0][0] != " ":
        immediate_msg = "\n- The completion should start with a whitespace character (` `). This tends to produce better results due to the tokenization we use. See https://platform.openai.com/docs/guides/fine-tuning/preparing-your-dataset for more details"
        optional_msg = "Add a whitespace character to the beginning of the completion"
        optional_fn = add_space_start
        name="completion_space_start",
def lower_case_validator(df: pd.DataFrame, column: Any) -> Remediation | None:
    This validator will suggest to lowercase the column values, if more than a third of letters are uppercase.
    def lower_case(x: Any) -> Any:
        x[column] = x[column].str.lower()
    count_upper = df[column].apply(lambda x: sum(1 for c in x if c.isalpha() and c.isupper())).sum()
    count_lower = df[column].apply(lambda x: sum(1 for c in x if c.isalpha() and c.islower())).sum()
    if count_upper * 2 > count_lower:
            name="lower_case",
            immediate_msg=f"\n- More than a third of your `{column}` column/key is uppercase. Uppercase {column}s tends to perform worse than a mixture of case encountered in normal language. We recommend to lower case the data if that makes sense in your domain. See https://platform.openai.com/docs/guides/fine-tuning/preparing-your-dataset for more details",
            optional_msg=f"Lowercase all your data in column/key `{column}`",
            optional_fn=lower_case,
def read_any_format(
    fname: str, fields: list[str] = ["prompt", "completion"]
) -> tuple[pd.DataFrame | None, Remediation]:
    This function will read a file saved in .csv, .json, .txt, .xlsx or .tsv format using pandas.
     - for .xlsx it will read the first sheet
     - for .txt it will assume completions and split on newline
    remediation = None
    df = None
    if os.path.isfile(fname):
            if fname.lower().endswith(".csv") or fname.lower().endswith(".tsv"):
                file_extension_str, separator = ("CSV", ",") if fname.lower().endswith(".csv") else ("TSV", "\t")
                immediate_msg = (
                    f"\n- Based on your file extension, your file is formatted as a {file_extension_str} file"
                necessary_msg = f"Your format `{file_extension_str}` will be converted to `JSONL`"
                df = pd.read_csv(fname, sep=separator, dtype=str).fillna("")
            elif fname.lower().endswith(".xlsx"):
                immediate_msg = "\n- Based on your file extension, your file is formatted as an Excel file"
                necessary_msg = "Your format `XLSX` will be converted to `JSONL`"
                xls = pd.ExcelFile(fname)
                sheets = xls.sheet_names
                if len(sheets) > 1:
                    immediate_msg += "\n- Your Excel file contains more than one sheet. Please either save as csv or ensure all data is present in the first sheet. WARNING: Reading only the first sheet..."
                df = pd.read_excel(fname, dtype=str).fillna("")
            elif fname.lower().endswith(".txt"):
                immediate_msg = "\n- Based on your file extension, you provided a text file"
                necessary_msg = "Your format `TXT` will be converted to `JSONL`"
                with open(fname, "r") as f:
                    df = pd.DataFrame(
                        [["", line] for line in content.split("\n")],
                        columns=fields,
                        dtype=str,
                    ).fillna("")
            elif fname.lower().endswith(".jsonl"):
                df = pd.read_json(fname, lines=True, dtype=str).fillna("")  # type: ignore
                if len(df) == 1:  # type: ignore
                    # this is NOT what we expect for a .jsonl file
                    immediate_msg = "\n- Your JSONL file appears to be in a JSON format. Your file will be converted to JSONL format"
                    necessary_msg = "Your format `JSON` will be converted to `JSONL`"
                    df = pd.read_json(fname, dtype=str).fillna("")  # type: ignore
                    pass  # this is what we expect for a .jsonl file
            elif fname.lower().endswith(".json"):
                    # to handle case where .json file is actually a .jsonl file
                        # this code path corresponds to a .json file that has one line
                        # this is NOT what we expect for a .json file
                        immediate_msg = "\n- Your JSON file appears to be in a JSONL format. Your file will be converted to JSONL format"
                    # this code path corresponds to a .json file that has multiple lines (i.e. it is indented)
                error_msg = (
                    "Your file must have one of the following extensions: .CSV, .TSV, .XLSX, .TXT, .JSON or .JSONL"
                if "." in fname:
                    error_msg += f" Your file `{fname}` ends with the extension `.{fname.split('.')[-1]}` which is not supported."
                    error_msg += f" Your file `{fname}` is missing a file extension."
        except (ValueError, TypeError):
            file_extension_str = fname.split(".")[-1].upper()
            error_msg = f"Your file `{fname}` does not appear to be in valid {file_extension_str} format. Please ensure your file is formatted as a valid {file_extension_str} file."
        error_msg = f"File {fname} does not exist."
    remediation = Remediation(
        name="read_any_format",
    return df, remediation
def format_inferrer_validator(df: pd.DataFrame) -> Remediation:
    This validator will infer the likely fine-tuning format of the data, and display it to the user if it is classification.
    It will also suggest to use ada and explain train/validation split benefits.
    if ft_type == "classification":
        immediate_msg = f"\n- Based on your data it seems like you're trying to fine-tune a model for {ft_type}\n- For classification, we recommend you try one of the faster and cheaper models, such as `ada`\n- For classification, you can estimate the expected model performance by keeping a held out dataset, which is not used for training"
def apply_necessary_remediation(df: OptionalDataFrameT, remediation: Remediation) -> OptionalDataFrameT:
    This function will apply a necessary remediation to a dataframe, or print an error message if one exists.
    if remediation.error_msg is not None:
        sys.stderr.write(f"\n\nERROR in {remediation.name} validator: {remediation.error_msg}\n\nAborting...")
    if remediation.immediate_msg is not None:
        sys.stdout.write(remediation.immediate_msg)
    if remediation.necessary_fn is not None:
        df = remediation.necessary_fn(df)
def accept_suggestion(input_text: str, auto_accept: bool) -> bool:
    sys.stdout.write(input_text)
    if auto_accept:
        sys.stdout.write("Y\n")
    return input().lower() != "n"
def apply_optional_remediation(
    df: pd.DataFrame, remediation: Remediation, auto_accept: bool
) -> tuple[pd.DataFrame, bool]:
    This function will apply an optional remediation to a dataframe, based on the user input.
    optional_applied = False
    input_text = f"- [Recommended] {remediation.optional_msg} [Y/n]: "
    if remediation.optional_msg is not None:
        if accept_suggestion(input_text, auto_accept):
            assert remediation.optional_fn is not None
            df = remediation.optional_fn(df)
            optional_applied = True
    if remediation.necessary_msg is not None:
        sys.stdout.write(f"- [Necessary] {remediation.necessary_msg}\n")
    return df, optional_applied
def estimate_fine_tuning_time(df: pd.DataFrame) -> None:
    Estimate the time it'll take to fine-tune the dataset
    ft_format = infer_task_type(df)
    expected_time = 1.0
    if ft_format == "classification":
        num_examples = len(df)
        expected_time = num_examples * 1.44
        size = df.memory_usage(index=True).sum()
        expected_time = size * 0.0515
    def format_time(time: float) -> str:
        if time < 60:
            return f"{round(time, 2)} seconds"
        elif time < 3600:
            return f"{round(time / 60, 2)} minutes"
        elif time < 86400:
            return f"{round(time / 3600, 2)} hours"
            return f"{round(time / 86400, 2)} days"
    time_string = format_time(expected_time + 140)
        f"Once your model starts training, it'll approximately take {time_string} to train a `curie` model, and less for `ada` and `babbage`. Queue will approximately take half an hour per job ahead of you.\n"
def get_outfnames(fname: str, split: bool) -> list[str]:
    suffixes = ["_train", "_valid"] if split else [""]
        index_suffix = f" ({i})" if i > 0 else ""
        candidate_fnames = [f"{os.path.splitext(fname)[0]}_prepared{suffix}{index_suffix}.jsonl" for suffix in suffixes]
        if not any(os.path.isfile(f) for f in candidate_fnames):
            return candidate_fnames
def get_classification_hyperparams(df: pd.DataFrame) -> tuple[int, object]:
    n_classes = df.completion.nunique()
    pos_class = None
    if n_classes == 2:
        pos_class = df.completion.value_counts().index[0]
    return n_classes, pos_class
def write_out_file(df: pd.DataFrame, fname: str, any_remediations: bool, auto_accept: bool) -> None:
    This function will write out a dataframe to a file, if the user would like to proceed, and also offer a fine-tuning command with the newly created file.
    For classification it will optionally ask the user if they would like to split the data into train/valid files, and modify the suggested command to include the valid set.
    common_prompt_suffix = get_common_xfix(df.prompt, xfix="suffix")
    common_completion_suffix = get_common_xfix(df.completion, xfix="suffix")
    split = False
    input_text = "- [Recommended] Would you like to split into training and validation set? [Y/n]: "
            split = True
    additional_params = ""
    common_prompt_suffix_new_line_handled = common_prompt_suffix.replace("\n", "\\n")
    common_completion_suffix_new_line_handled = common_completion_suffix.replace("\n", "\\n")
    optional_ending_string = (
        f' Make sure to include `stop=["{common_completion_suffix_new_line_handled}"]` so that the generated texts ends at the expected place.'
        if len(common_completion_suffix_new_line_handled) > 0
        else ""
    input_text = "\n\nYour data will be written to a new JSONL file. Proceed [Y/n]: "
    if not any_remediations and not split:
            f'\nYou can use your file for fine-tuning:\n> openai api fine_tunes.create -t "{fname}"{additional_params}\n\nAfter you’ve fine-tuned a model, remember that your prompt has to end with the indicator string `{common_prompt_suffix_new_line_handled}` for the model to start generating completions, rather than continuing with the prompt.{optional_ending_string}\n'
        estimate_fine_tuning_time(df)
    elif accept_suggestion(input_text, auto_accept):
        fnames = get_outfnames(fname, split)
        if split:
            assert len(fnames) == 2 and "train" in fnames[0] and "valid" in fnames[1]
            MAX_VALID_EXAMPLES = 1000
            n_train = max(len(df) - MAX_VALID_EXAMPLES, int(len(df) * 0.8))
            df_train = df.sample(n=n_train, random_state=42)
            df_valid = df.drop(df_train.index)
            df_train[["prompt", "completion"]].to_json(  # type: ignore
                fnames[0], lines=True, orient="records", force_ascii=False, indent=None
            df_valid[["prompt", "completion"]].to_json(
                fnames[1], lines=True, orient="records", force_ascii=False, indent=None
            n_classes, pos_class = get_classification_hyperparams(df)
            additional_params += " --compute_classification_metrics"
                additional_params += f' --classification_positive_class "{pos_class}"'
                additional_params += f" --classification_n_classes {n_classes}"
            assert len(fnames) == 1
            df[["prompt", "completion"]].to_json(
        # Add -v VALID_FILE if we split the file into train / valid
        files_string = ("s" if split else "") + " to `" + ("` and `".join(fnames))
        valid_string = f' -v "{fnames[1]}"' if split else ""
        separator_reminder = (
            if len(common_prompt_suffix_new_line_handled) == 0
            else f"After you’ve fine-tuned a model, remember that your prompt has to end with the indicator string `{common_prompt_suffix_new_line_handled}` for the model to start generating completions, rather than continuing with the prompt."
            f'\nWrote modified file{files_string}`\nFeel free to take a look!\n\nNow use that file when fine-tuning:\n> openai api fine_tunes.create -t "{fnames[0]}"{valid_string}{additional_params}\n\n{separator_reminder}{optional_ending_string}\n'
        sys.stdout.write("Aborting... did not write the file\n")
def infer_task_type(df: pd.DataFrame) -> str:
    Infer the likely fine-tuning task type from the data
    CLASSIFICATION_THRESHOLD = 3  # min_average instances of each class
    if sum(df.prompt.str.len()) == 0:
        return "open-ended generation"
    if len(df.completion.unique()) < len(df) / CLASSIFICATION_THRESHOLD:
        return "classification"
    return "conditional generation"
def get_common_xfix(series: Any, xfix: str = "suffix") -> str:
    Finds the longest common suffix or prefix of all the values in a series
    common_xfix = ""
        common_xfixes = (
            series.str[-(len(common_xfix) + 1) :] if xfix == "suffix" else series.str[: len(common_xfix) + 1]
        )  # first few or last few characters
        if common_xfixes.nunique() != 1:  # we found the character at which we don't have a unique xfix anymore
        elif common_xfix == common_xfixes.values[0]:  # the entire first row is a prefix of every other row
        else:  # the first or last few characters are still common across all rows - let's try to add one more
            common_xfix = common_xfixes.values[0]
    return common_xfix
Validator: TypeAlias = "Callable[[pd.DataFrame], Remediation | None]"
def get_validators() -> list[Validator]:
        num_examples_validator,
        lambda x: necessary_column_validator(x, "prompt"),
        lambda x: necessary_column_validator(x, "completion"),
        additional_column_validator,
        non_empty_field_validator,
        format_inferrer_validator,
        duplicated_rows_validator,
        long_examples_validator,
        lambda x: lower_case_validator(x, "prompt"),
        lambda x: lower_case_validator(x, "completion"),
        common_prompt_suffix_validator,
        common_prompt_prefix_validator,
        common_completion_prefix_validator,
        common_completion_suffix_validator,
        completions_space_start_validator,
def apply_validators(
    df: pd.DataFrame,
    fname: str,
    remediation: Remediation | None,
    validators: list[Validator],
    auto_accept: bool,
    write_out_file_func: Callable[..., Any],
    optional_remediations: list[Remediation] = []
    if remediation is not None:
        optional_remediations.append(remediation)
    for validator in validators:
        remediation = validator(df)
            df = apply_necessary_remediation(df, remediation)
    any_optional_or_necessary_remediations = any(
            remediation
            for remediation in optional_remediations
            if remediation.optional_msg is not None or remediation.necessary_msg is not None
    any_necessary_applied = any(
        [remediation for remediation in optional_remediations if remediation.necessary_msg is not None]
    any_optional_applied = False
    if any_optional_or_necessary_remediations:
        sys.stdout.write("\n\nBased on the analysis we will perform the following actions:\n")
        for remediation in optional_remediations:
            df, optional_applied = apply_optional_remediation(df, remediation, auto_accept)
            any_optional_applied = any_optional_applied or optional_applied
        sys.stdout.write("\n\nNo remediations found.\n")
    any_optional_or_necessary_applied = any_optional_applied or any_necessary_applied
    write_out_file_func(df, fname, any_optional_or_necessary_applied, auto_accept)
"""Validator functions for standard library types.
Import of this module is deferred since it contains imports of many standard library modules.
from fractions import Fraction
from ipaddress import IPv4Address, IPv4Interface, IPv4Network, IPv6Address, IPv6Interface, IPv6Network
from typing import Any, Callable, TypeVar, Union, cast
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError
from pydantic_core import PydanticCustomError, PydanticKnownError, core_schema
from typing_extensions import get_args, get_origin
from typing_inspection import typing_objects
from pydantic._internal._import_utils import import_cached_field_info
from pydantic.errors import PydanticSchemaGenerationError
def sequence_validator(
    input_value: Sequence[Any],
    validator: core_schema.ValidatorFunctionWrapHandler,
) -> Sequence[Any]:
    """Validator for `Sequence` types, isinstance(v, Sequence) has already been called."""
    value_type = type(input_value)
    # We don't accept any plain string as a sequence
    # Relevant issue: https://github.com/pydantic/pydantic/issues/5595
    if issubclass(value_type, (str, bytes)):
        raise PydanticCustomError(
            'sequence_str',
            "'{type_name}' instances are not allowed as a Sequence value",
            {'type_name': value_type.__name__},
    # TODO: refactor sequence validation to validate with either a list or a tuple
    # schema, depending on the type of the value.
    # Additionally, we should be able to remove one of either this validator or the
    # SequenceValidator in _std_types_schema.py (preferably this one, while porting over some logic).
    # Effectively, a refactor for sequence validation is needed.
    if value_type is tuple:
        input_value = list(input_value)
    v_list = validator(input_value)
    # the rest of the logic is just re-creating the original type from `v_list`
    if value_type is list:
        return v_list
    elif issubclass(value_type, range):
        # return the list as we probably can't re-create the range
    elif value_type is tuple:
        return tuple(v_list)
        # best guess at how to re-create the original type, more custom construction logic might be required
        return value_type(v_list)  # type: ignore[call-arg]
def import_string(value: Any) -> Any:
            return _import_string_logic(value)
            raise PydanticCustomError('import_error', 'Invalid python path: {error}', {'error': str(e)}) from e
        # otherwise we just return the value and let the next validator do the rest of the work
def _import_string_logic(dotted_path: str) -> Any:
    """Inspired by uvicorn — dotted paths should include a colon before the final item if that item is not a module.
    (This is necessary to distinguish between a submodule and an attribute when there is a conflict.).
    If the dotted path does not include a colon and the final item is not a valid module, importing as an attribute
    rather than a submodule will be attempted automatically.
    So, for example, the following values of `dotted_path` result in the following returned values:
    * 'collections': <module 'collections'>
    * 'collections.abc': <module 'collections.abc'>
    * 'collections.abc:Mapping': <class 'collections.abc.Mapping'>
    * `collections.abc.Mapping`: <class 'collections.abc.Mapping'> (though this is a bit slower than the previous line)
    An error will be raised under any of the following scenarios:
    * `dotted_path` contains more than one colon (e.g., 'collections:abc:Mapping')
    * the substring of `dotted_path` before the colon is not a valid module in the environment (e.g., '123:Mapping')
    * the substring of `dotted_path` after the colon is not an attribute of the module (e.g., 'collections:abc123')
    components = dotted_path.strip().split(':')
    if len(components) > 2:
        raise ImportError(f"Import strings should have at most one ':'; received {dotted_path!r}")
    module_path = components[0]
    if not module_path:
        raise ImportError(f'Import strings should have a nonempty module name; received {dotted_path!r}')
        if '.' in module_path:
            # Check if it would be valid if the final item was separated from its module with a `:`
            maybe_module_path, maybe_attribute = dotted_path.strip().rsplit('.', 1)
                return _import_string_logic(f'{maybe_module_path}:{maybe_attribute}')
            raise ImportError(f'No module named {module_path!r}') from e
    if len(components) > 1:
        attribute = components[1]
            return getattr(module, attribute)
            raise ImportError(f'cannot import name {attribute!r} from {module_path!r}') from e
def pattern_either_validator(input_value: Any, /) -> re.Pattern[Any]:
    if isinstance(input_value, re.Pattern):
        return input_value
    elif isinstance(input_value, (str, bytes)):
        # todo strict mode
        return compile_pattern(input_value)  # type: ignore
        raise PydanticCustomError('pattern_type', 'Input should be a valid pattern')
def pattern_str_validator(input_value: Any, /) -> re.Pattern[str]:
        if isinstance(input_value.pattern, str):
            raise PydanticCustomError('pattern_str_type', 'Input should be a string pattern')
    elif isinstance(input_value, str):
        return compile_pattern(input_value)
    elif isinstance(input_value, bytes):
def pattern_bytes_validator(input_value: Any, /) -> re.Pattern[bytes]:
        if isinstance(input_value.pattern, bytes):
            raise PydanticCustomError('pattern_bytes_type', 'Input should be a bytes pattern')
PatternType = TypeVar('PatternType', str, bytes)
def compile_pattern(pattern: PatternType) -> re.Pattern[PatternType]:
        return re.compile(pattern)
        raise PydanticCustomError('pattern_regex', 'Input should be a valid regular expression')
def ip_v4_address_validator(input_value: Any, /) -> IPv4Address:
    if isinstance(input_value, IPv4Address):
        return IPv4Address(input_value)
        raise PydanticCustomError('ip_v4_address', 'Input is not a valid IPv4 address')
def ip_v6_address_validator(input_value: Any, /) -> IPv6Address:
    if isinstance(input_value, IPv6Address):
        return IPv6Address(input_value)
        raise PydanticCustomError('ip_v6_address', 'Input is not a valid IPv6 address')
def ip_v4_network_validator(input_value: Any, /) -> IPv4Network:
    """Assume IPv4Network initialised with a default `strict` argument.
    See more:
    https://docs.python.org/library/ipaddress.html#ipaddress.IPv4Network
    if isinstance(input_value, IPv4Network):
        return IPv4Network(input_value)
        raise PydanticCustomError('ip_v4_network', 'Input is not a valid IPv4 network')
def ip_v6_network_validator(input_value: Any, /) -> IPv6Network:
    """Assume IPv6Network initialised with a default `strict` argument.
    https://docs.python.org/library/ipaddress.html#ipaddress.IPv6Network
    if isinstance(input_value, IPv6Network):
        return IPv6Network(input_value)
        raise PydanticCustomError('ip_v6_network', 'Input is not a valid IPv6 network')
def ip_v4_interface_validator(input_value: Any, /) -> IPv4Interface:
    if isinstance(input_value, IPv4Interface):
        return IPv4Interface(input_value)
        raise PydanticCustomError('ip_v4_interface', 'Input is not a valid IPv4 interface')
def ip_v6_interface_validator(input_value: Any, /) -> IPv6Interface:
    if isinstance(input_value, IPv6Interface):
        return IPv6Interface(input_value)
        raise PydanticCustomError('ip_v6_interface', 'Input is not a valid IPv6 interface')
def fraction_validator(input_value: Any, /) -> Fraction:
    if isinstance(input_value, Fraction):
        return Fraction(input_value)
        raise PydanticCustomError('fraction_parsing', 'Input is not a valid fraction')
def forbid_inf_nan_check(x: Any) -> Any:
    if not math.isfinite(x):
        raise PydanticKnownError('finite_number')
def _safe_repr(v: Any) -> int | float | str:
    """The context argument for `PydanticKnownError` requires a number or str type, so we do a simple repr() coercion for types like timedelta.
    See tests/test_types.py::test_annotated_metadata_any_order for some context.
    if isinstance(v, (int, float, str)):
def greater_than_validator(x: Any, gt: Any) -> Any:
        if not (x > gt):
            raise PydanticKnownError('greater_than', {'gt': _safe_repr(gt)})
        raise TypeError(f"Unable to apply constraint 'gt' to supplied value {x}")
def greater_than_or_equal_validator(x: Any, ge: Any) -> Any:
        if not (x >= ge):
            raise PydanticKnownError('greater_than_equal', {'ge': _safe_repr(ge)})
        raise TypeError(f"Unable to apply constraint 'ge' to supplied value {x}")
def less_than_validator(x: Any, lt: Any) -> Any:
        if not (x < lt):
            raise PydanticKnownError('less_than', {'lt': _safe_repr(lt)})
        raise TypeError(f"Unable to apply constraint 'lt' to supplied value {x}")
def less_than_or_equal_validator(x: Any, le: Any) -> Any:
        if not (x <= le):
            raise PydanticKnownError('less_than_equal', {'le': _safe_repr(le)})
        raise TypeError(f"Unable to apply constraint 'le' to supplied value {x}")
def multiple_of_validator(x: Any, multiple_of: Any) -> Any:
        if x % multiple_of:
            raise PydanticKnownError('multiple_of', {'multiple_of': _safe_repr(multiple_of)})
        raise TypeError(f"Unable to apply constraint 'multiple_of' to supplied value {x}")
def min_length_validator(x: Any, min_length: Any) -> Any:
        if not (len(x) >= min_length):
            raise PydanticKnownError(
                'too_short', {'field_type': 'Value', 'min_length': min_length, 'actual_length': len(x)}
        raise TypeError(f"Unable to apply constraint 'min_length' to supplied value {x}")
def max_length_validator(x: Any, max_length: Any) -> Any:
        if len(x) > max_length:
                'too_long',
                {'field_type': 'Value', 'max_length': max_length, 'actual_length': len(x)},
        raise TypeError(f"Unable to apply constraint 'max_length' to supplied value {x}")
def _extract_decimal_digits_info(decimal: Decimal) -> tuple[int, int]:
    """Compute the total number of digits and decimal places for a given [`Decimal`][decimal.Decimal] instance.
    This function handles both normalized and non-normalized Decimal instances.
    Example: Decimal('1.230') -> 4 digits, 3 decimal places
        decimal (Decimal): The decimal number to analyze.
        tuple[int, int]: A tuple containing the number of decimal places and total digits.
    Though this could be divided into two separate functions, the logic is easier to follow if we couple the computation
    of the number of decimals and digits together.
        decimal_tuple = decimal.as_tuple()
        assert isinstance(decimal_tuple.exponent, int)
        exponent = decimal_tuple.exponent
        num_digits = len(decimal_tuple.digits)
        if exponent >= 0:
            # A positive exponent adds that many trailing zeros
            # Ex: digit_tuple=(1, 2, 3), exponent=2 -> 12300 -> 0 decimal places, 5 digits
            num_digits += exponent
            decimal_places = 0
            # If the absolute value of the negative exponent is larger than the
            # number of digits, then it's the same as the number of digits,
            # because it'll consume all the digits in digit_tuple and then
            # add abs(exponent) - len(digit_tuple) leading zeros after the decimal point.
            # Ex: digit_tuple=(1, 2, 3), exponent=-2 -> 1.23 -> 2 decimal places, 3 digits
            # Ex: digit_tuple=(1, 2, 3), exponent=-4 -> 0.0123 -> 4 decimal places, 4 digits
            decimal_places = abs(exponent)
            num_digits = max(num_digits, decimal_places)
        return decimal_places, num_digits
    except (AssertionError, AttributeError):
        raise TypeError(f'Unable to extract decimal digits info from supplied value {decimal}')
def max_digits_validator(x: Any, max_digits: Any) -> Any:
        _, num_digits = _extract_decimal_digits_info(x)
        _, normalized_num_digits = _extract_decimal_digits_info(x.normalize())
        if (num_digits > max_digits) and (normalized_num_digits > max_digits):
                'decimal_max_digits',
                {'max_digits': max_digits},
        raise TypeError(f"Unable to apply constraint 'max_digits' to supplied value {x}")
def decimal_places_validator(x: Any, decimal_places: Any) -> Any:
        decimal_places_, _ = _extract_decimal_digits_info(x)
        if decimal_places_ > decimal_places:
            normalized_decimal_places, _ = _extract_decimal_digits_info(x.normalize())
            if normalized_decimal_places > decimal_places:
                    'decimal_max_places',
                    {'decimal_places': decimal_places},
        raise TypeError(f"Unable to apply constraint 'decimal_places' to supplied value {x}")
def deque_validator(input_value: Any, handler: core_schema.ValidatorFunctionWrapHandler) -> collections.deque[Any]:
    return collections.deque(handler(input_value), maxlen=getattr(input_value, 'maxlen', None))
def defaultdict_validator(
    input_value: Any, handler: core_schema.ValidatorFunctionWrapHandler, default_default_factory: Callable[[], Any]
) -> collections.defaultdict[Any, Any]:
    if isinstance(input_value, collections.defaultdict):
        default_factory = input_value.default_factory
        return collections.defaultdict(default_factory, handler(input_value))
        return collections.defaultdict(default_default_factory, handler(input_value))
def get_defaultdict_default_default_factory(values_source_type: Any) -> Callable[[], Any]:
    FieldInfo = import_cached_field_info()
    values_type_origin = get_origin(values_source_type)
    def infer_default() -> Callable[[], Any]:
        allowed_default_types: dict[Any, Any] = {
            collections.abc.Sequence: tuple,
            collections.abc.MutableSequence: list,
            typing.Sequence: list,
            typing.MutableSet: set,
            collections.abc.MutableSet: set,
            collections.abc.Set: frozenset,
            typing.MutableMapping: dict,
            typing.Mapping: dict,
            collections.abc.Mapping: dict,
            collections.abc.MutableMapping: dict,
            float: float,
            int: int,
            str: str,
            bool: bool,
        values_type = values_type_origin or values_source_type
        instructions = 'set using `DefaultDict[..., Annotated[..., Field(default_factory=...)]]`'
        if typing_objects.is_typevar(values_type):
            def type_var_default_factory() -> None:
                    'Generic defaultdict cannot be used without a concrete value type or an'
                    ' explicit default factory, ' + instructions
            return type_var_default_factory
        elif values_type not in allowed_default_types:
            # a somewhat subjective set of types that have reasonable default values
            allowed_msg = ', '.join([t.__name__ for t in set(allowed_default_types.values())])
            raise PydanticSchemaGenerationError(
                f'Unable to infer a default factory for keys of type {values_source_type}.'
                f' Only {allowed_msg} are supported, other types require an explicit default factory'
                ' ' + instructions
        return allowed_default_types[values_type]
    # Assume Annotated[..., Field(...)]
    if typing_objects.is_annotated(values_type_origin):
        field_info = next((v for v in get_args(values_source_type) if isinstance(v, FieldInfo)), None)
    if field_info and field_info.default_factory:
        # Assume the default factory does not take any argument:
        default_default_factory = cast(Callable[[], Any], field_info.default_factory)
        default_default_factory = infer_default()
    return default_default_factory
def validate_str_is_valid_iana_tz(value: Any, /) -> ZoneInfo:
    if isinstance(value, ZoneInfo):
        return ZoneInfo(value)
    except (ZoneInfoNotFoundError, ValueError, TypeError):
        raise PydanticCustomError('zoneinfo_str', 'invalid timezone: {value}', {'value': value})
NUMERIC_VALIDATOR_LOOKUP: dict[str, Callable] = {
    'gt': greater_than_validator,
    'ge': greater_than_or_equal_validator,
    'lt': less_than_validator,
    'le': less_than_or_equal_validator,
    'multiple_of': multiple_of_validator,
    'min_length': min_length_validator,
    'max_length': max_length_validator,
    'max_digits': max_digits_validator,
    'decimal_places': decimal_places_validator,
IpType = Union[IPv4Address, IPv6Address, IPv4Network, IPv6Network, IPv4Interface, IPv6Interface]
IP_VALIDATOR_LOOKUP: dict[type[IpType], Callable] = {
    IPv4Address: ip_v4_address_validator,
    IPv6Address: ip_v6_address_validator,
    IPv4Network: ip_v4_network_validator,
    IPv6Network: ip_v6_network_validator,
    IPv4Interface: ip_v4_interface_validator,
    IPv6Interface: ip_v6_interface_validator,
MAPPING_ORIGIN_MAP: dict[Any, Any] = {
    typing.DefaultDict: collections.defaultdict,  # noqa: UP006
    collections.defaultdict: collections.defaultdict,
    typing.OrderedDict: collections.OrderedDict,  # noqa: UP006
    collections.OrderedDict: collections.OrderedDict,
    typing_extensions.OrderedDict: collections.OrderedDict,
    typing.Counter: collections.Counter,
    collections.Counter: collections.Counter,
    # this doesn't handle subclasses of these
    # parametrized typing.{Mutable}Mapping creates one of these
"""Contains utilities to validate argument values in `huggingface_hub`."""
from huggingface_hub.errors import HFValidationError
from ._typing import CallableT
REPO_ID_REGEX = re.compile(
    (\b[\w\-.]+\b/)? # optional namespace (username or organization)
    \b               # starts with a word boundary
    [\w\-.]{1,96}    # repo_name: alphanumeric + . _ -
    \b               # ends with a word boundary
    flags=re.VERBOSE,
def validate_hf_hub_args(fn: CallableT) -> CallableT:
    """Validate values received as argument for any public method of `huggingface_hub`.
    The goal of this decorator is to harmonize validation of arguments reused
    everywhere. By default, all defined validators are tested.
    Validators:
        - [`~utils.validate_repo_id`]: `repo_id` must be `"repo_name"`
          or `"namespace/repo_name"`. Namespace is a username or an organization.
        - [`~utils.smoothly_deprecate_legacy_arguments`]: Ignore `proxies` when downloading files (should be set globally).
    >>> from huggingface_hub.utils import validate_hf_hub_args
    >>> @validate_hf_hub_args
    ... def my_cool_method(repo_id: str):
    ...     print(repo_id)
    >>> my_cool_method(repo_id="valid_repo_id")
    valid_repo_id
    >>> my_cool_method("other..repo..id")
    huggingface_hub.utils._validators.HFValidationError: Cannot have -- or .. in repo_id: 'other..repo..id'.
    >>> my_cool_method(repo_id="other..repo..id")
        [`~utils.HFValidationError`]:
            If an input is not valid.
    # TODO: add an argument to opt-out validation for specific argument?
    signature = inspect.signature(fn)
    def _inner_fn(*args, **kwargs):
        for arg_name, arg_value in chain(
            zip(signature.parameters, args),  # Args values
            kwargs.items(),  # Kwargs values
            if arg_name in ["repo_id", "from_id", "to_id"]:
                validate_repo_id(arg_value)
        kwargs = smoothly_deprecate_legacy_arguments(fn_name=fn.__name__, kwargs=kwargs)
    return _inner_fn  # type: ignore
def validate_repo_id(repo_id: str) -> None:
    """Validate `repo_id` is valid.
    This is not meant to replace the proper validation made on the Hub but rather to
    avoid local inconsistencies whenever possible (example: passing `repo_type` in the
    `repo_id` is forbidden).
    Rules:
    - Between 1 and 96 characters.
    - Either "repo_name" or "namespace/repo_name"
    - [a-zA-Z0-9] or "-", "_", "."
    - "--" and ".." are forbidden
    Valid: `"foo"`, `"foo/bar"`, `"123"`, `"Foo-BAR_foo.bar123"`
    Not valid: `"datasets/foo/bar"`, `".repo_id"`, `"foo--bar"`, `"foo.git"`
    >>> from huggingface_hub.utils import validate_repo_id
    >>> validate_repo_id(repo_id="valid_repo_id")
    >>> validate_repo_id(repo_id="other..repo..id")
    Discussed in https://github.com/huggingface/huggingface_hub/issues/1008.
    In moon-landing (internal repository):
    - https://github.com/huggingface/moon-landing/blob/main/server/lib/Names.ts#L27
    - https://github.com/huggingface/moon-landing/blob/main/server/views/components/NewRepoForm/NewRepoForm.svelte#L138
    if not isinstance(repo_id, str):
        # Typically, a Path is not a repo_id
        raise HFValidationError(f"Repo id must be a string, not {type(repo_id)}: '{repo_id}'.")
    if repo_id.count("/") > 1:
        raise HFValidationError(
            "Repo id must be in the form 'repo_name' or 'namespace/repo_name':"
            f" '{repo_id}'. Use `repo_type` argument if needed."
    if not REPO_ID_REGEX.match(repo_id):
            "Repo id must use alphanumeric chars, '-', '_' or '.'."
            " The name cannot start or end with '-' or '.' and the maximum length is 96:"
            f" '{repo_id}'."
    if "--" in repo_id or ".." in repo_id:
        raise HFValidationError(f"Cannot have -- or .. in repo_id: '{repo_id}'.")
    if repo_id.endswith(".git"):
        raise HFValidationError(f"Repo_id cannot end by '.git': '{repo_id}'.")
def smoothly_deprecate_legacy_arguments(fn_name: str, kwargs: dict[str, Any]) -> dict[str, Any]:
    """Smoothly deprecate legacy arguments in the `huggingface_hub` codebase.
    This function ignores some deprecated arguments from the kwargs and warns the user they are ignored.
    The goal is to avoid breaking existing code while guiding the user to the new way of doing things.
    List of deprecated arguments:
        - `proxies`:
            To set up proxies, user must either use the HTTP_PROXY environment variable or configure the `httpx.Client`
            manually using the [`set_client_factory`] function.
            In huggingface_hub 0.x, `proxies` was a dictionary directly passed to `requests.request`.
            In huggingface_hub 1.x, we migrated to `httpx` which does not support `proxies` the same way.
            In particular, it is not possible to configure proxies on a per-request basis. The solution is to configure
            it globally using the [`set_client_factory`] function or using the HTTP_PROXY environment variable.
            For more details, see:
            - https://www.python-httpx.org/advanced/proxies/
            - https://www.python-httpx.org/compatibility/#proxy-keys.
        - `resume_download`: deprecated without replacement. `huggingface_hub` always resumes downloads whenever possible.
        - `force_filename`: deprecated without replacement. Filename is always the same as on the Hub.
        - `local_dir_use_symlinks`: deprecated without replacement. Downloading to a local directory does not use symlinks anymore.
    new_kwargs = kwargs.copy()  # do not mutate input !
    # proxies
    proxies = new_kwargs.pop("proxies", None)  # remove from kwargs
    if proxies is not None:
            f"The `proxies` argument is ignored in `{fn_name}`. To set up proxies, use the HTTP_PROXY / HTTPS_PROXY"
            " environment variables or configure the `httpx.Client` manually using `huggingface_hub.set_client_factory`."
            " See https://www.python-httpx.org/advanced/proxies/ for more details."
    # resume_download
    resume_download = new_kwargs.pop("resume_download", None)  # remove from kwargs
    if resume_download is not None:
            f"The `resume_download` argument is deprecated and ignored in `{fn_name}`. Downloads always resume"
            " whenever possible."
    # force_filename
    force_filename = new_kwargs.pop("force_filename", None)  # remove from kwargs
    if force_filename is not None:
            f"The `force_filename` argument is deprecated and ignored in `{fn_name}`. Filename is always the same "
            "as on the Hub."
    # local_dir_use_symlinks
    local_dir_use_symlinks = new_kwargs.pop("local_dir_use_symlinks", None)  # remove from kwargs
    if local_dir_use_symlinks is not None:
            f"The `local_dir_use_symlinks` argument is deprecated and ignored in `{fn_name}`. Downloading to a local"
            " directory does not use symlinks anymore."
    return new_kwargs
