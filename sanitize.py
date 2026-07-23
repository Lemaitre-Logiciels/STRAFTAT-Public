import os
import re
import stat
import subprocess
import shutil
import tempfile
import time
import traceback

DIRECTORIES = [
    # Odin Inspector (v3.0.4)
    r"STRAFTAT\Assets\Plugins\Sirenix",
    # Bakery GPU Lightmapper
    r"STRAFTAT\Assets\Bakery",
    # Curvy (v7.1.8)
    r"STRAFTAT\Assets\Plugins\Curvy",
    r"STRAFTAT\Assets\Plugins\Curvy Examples",
    r"STRAFTAT\Assets\Plugins\Curvy Resources",
    r"STRAFTAT\Assets\Plugins\DevTools",
    r"STRAFTAT\Assets\Plugins\LibTessDotNet",
    # Amplify Shader Editor
    r"STRAFTAT\Assets\Plugins\AmplifyShaderEditor",
    # AllSky
    r"STRAFTAT\Assets\Allsky",
    # FXVille Blood Pack
    r"STRAFTAT\Assets\FXVille_BloodPack",
    # Volumetric Light Beam (v1890)
    r"STRAFTAT\Assets\Plugins\VolumetricLightBeam",
    # Grabbit (v2022.0.2)
    r"STRAFTAT\Assets\Plugins\Grabbit",
    # Colorful Fog
    r"STRAFTAT\Assets\Plugins\Colorful Fog",
    # Easy Decal
    r"STRAFTAT\Assets\Sycoforge",
    # Shatterable Glass
    r"STRAFTAT\Assets\ShatterableGlass",
    # Real-Time Procedural Cable Simple (v1.1)
    r"STRAFTAT\Assets\Real-Time Procedural Cable Simple",
    # VFX Explosion Texture Pack
    r"STRAFTAT\Assets\VFX\VFX Explosion Texture Pack",
    # Rainbow Folders
    r"STRAFTAT\Assets\Plugins\RainbowFolders",
    # Heathen Steamworks Complete (v3.4.3)
    r"STRAFTAT\Assets\Plugins\Steamworks",
    # Heathen PhysKit Complete (v3.0.5)
    r"STRAFTAT\Assets\Plugins\com.heathen.physkit",
    # PlayFlow Cloud
    r"STRAFTAT\Assets\PlayFlowCloud",
    # Low Poly Guns - Gun models
    r"STRAFTAT\Assets\Weapons\Low Poly Guns",
    # DOTween
    r"STRAFTAT\Assets\Plugins\Demigiant",
    # FishNet (v3.10.8R)
    r"STRAFTAT\Assets\FishNet",
    # WarFX (v1.8.04)
    r"STRAFTAT\Assets\VFX\WarFX",
    # AllSky Free
    r"STRAFTAT\Assets\Environment\AllSkyFree",
    # LambdaTheDev NetworkAudioSync
    r"STRAFTAT\Assets\Plugins\LambdaTheDev",
    # Easy Decal editor resources
    r"STRAFTAT\Assets\Editor Default Resources\nu Assets",
    r"STRAFTAT\Assets\Gizmos",
    # Bakery GPU Lightmapper
    r"STRAFTAT\Assets\Editor\x64\Bakery",
    # Heathen Steamworks Complete
    r"STRAFTAT\Assets\Gameplay\SteamPrefabs",
    # Unity Standard Assets
    r"STRAFTAT\Assets\Gameplay\Standard Assets",
    # Unity Particle Pack
    r"STRAFTAT\Assets\VFX\ParticlePack",
]

MAX_PUSH_BATCH_BYTES = 2 * 1024 * 1024 * 1024
MAX_PUSH_BATCH_FILES = 1500
MAX_PUSH_RETRIES = 3
PUSH_RETRY_DELAY_SECONDS = 5
REMOTE_URL = "https://github.com/Lemaitre-Logiciels/STRAFTAT-Public"
IMPORT_COMMIT_RE = re.compile(r"^Import batch (\d+)/(\d+)$")


def nuke(directory):
    if not os.path.isdir(directory):
        return

    for root, dirs, files in os.walk(directory, topdown=False):
        for f in files:
            if not f.endswith(".meta"):
                os.remove(os.path.join(root, f))
        if not os.listdir(root):
            os.rmdir(root)

    print(f"Nuked: {directory}")


def merge_leonadino():
    repo_dir = os.path.dirname(os.path.abspath(__file__))
    print("=== Merging origin/leonadino ===\n")
    try:
        subprocess.run(["git", "fetch", "origin", "leonadino"], cwd=repo_dir, check=True)
        subprocess.run(["git", "merge", "origin/leonadino", "--no-edit"], cwd=repo_dir, check=True)
        print("Merge successful.\n")
    except subprocess.CalledProcessError as e:
        print(f"Merge failed: {e}\n")
        raise


def clean_git_repo(repo_dir):
    print("=== Cleaning git repo (untracked + ignored files) ===\n")
    try:
        subprocess.run(["git", "clean", "-ffdx"], cwd=repo_dir, check=True)
        print("Git clean successful.\n")
    except subprocess.CalledProcessError as e:
        print(f"Git clean failed: {e}\n")
        raise


def reset_git_repo(repo_dir):
    print("=== Resetting tracked files to HEAD ===\n")
    try:
        subprocess.run(["git", "reset", "--hard", "HEAD"], cwd=repo_dir, check=True)
        print("Git reset successful.\n")
    except subprocess.CalledProcessError as e:
        print(f"Git reset failed: {e}\n")
        raise


def is_git_repo(repo_dir):
    result = subprocess.run(
        ["git", "rev-parse", "--is-inside-work-tree"],
        cwd=repo_dir,
        capture_output=True,
        text=True,
    )
    return result.returncode == 0 and result.stdout.strip() == "true"


def get_origin_url(repo_dir):
    result = subprocess.run(
        ["git", "remote", "get-url", "origin"],
        cwd=repo_dir,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        return None

    return result.stdout.strip()


def get_last_imported_batch(repo_dir):
    if not is_git_repo(repo_dir):
        return None, None

    result = subprocess.run(
        ["git", "log", "--format=%s"],
        cwd=repo_dir,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        return None, None

    last_batch = None
    total_batches = None
    for line in result.stdout.splitlines():
        match = IMPORT_COMMIT_RE.match(line.strip())
        if not match:
            continue

        batch_index = int(match.group(1))
        batch_total = int(match.group(2))
        if last_batch is None or batch_index > last_batch:
            last_batch = batch_index
            total_batches = batch_total

    return last_batch, total_batches


def should_resume_import(repo_dir, remote_url):
    if not is_git_repo(repo_dir):
        return False

    if get_origin_url(repo_dir) != remote_url:
        return False

    last_batch, _ = get_last_imported_batch(repo_dir)
    return last_batch is not None


def iter_repo_files(repo_dir):
    repo_files = []
    for root, dirs, files in os.walk(repo_dir):
        dirs[:] = [d for d in dirs if d != ".git"]
        for name in files:
            full_path = os.path.join(root, name)
            rel_path = os.path.relpath(full_path, repo_dir).replace("\\", "/")
            repo_files.append((rel_path, os.path.getsize(full_path)))

    repo_files.sort(key=lambda item: item[0])
    return repo_files


def build_push_batches(repo_files):
    batches = []
    current_batch = []
    current_size = 0

    for rel_path, size in repo_files:
        would_overflow_files = len(current_batch) >= MAX_PUSH_BATCH_FILES
        would_overflow_size = current_batch and current_size + size > MAX_PUSH_BATCH_BYTES

        if would_overflow_files or would_overflow_size:
            batches.append(current_batch)
            current_batch = []
            current_size = 0

        current_batch.append(rel_path)
        current_size += size

    if current_batch:
        batches.append(current_batch)

    return batches


def git_add_batch(repo_dir, batch_paths):
    with tempfile.NamedTemporaryFile("wb", delete=False) as pathspec_file:
        pathspec_file.write(b"\0".join(path.encode("utf-8") for path in batch_paths))
        pathspec_path = pathspec_file.name

    try:
        subprocess.run(
            ["git", "add", "--force", f"--pathspec-from-file={pathspec_path}", "--pathspec-file-nul"],
            cwd=repo_dir,
            check=True,
        )
    finally:
        os.remove(pathspec_path)


def run_with_retries(command, repo_dir, label, max_attempts=MAX_PUSH_RETRIES, delay_seconds=PUSH_RETRY_DELAY_SECONDS):
    for attempt in range(1, max_attempts + 1):
        try:
            subprocess.run(command, cwd=repo_dir, check=True)
            return
        except subprocess.CalledProcessError as e:
            if attempt == max_attempts:
                print(f"{label} failed after {attempt} attempts.")
                raise

            print(f"{label} failed on attempt {attempt}/{max_attempts}: {e}")
            print(f"Retrying in {delay_seconds} seconds...")
            time.sleep(delay_seconds)


def push_batch(repo_dir, index, total_batches):
    push_command = ["git", "push", "origin", "main"]
    if index == 1:
        push_command = ["git", "push", "--force", "-u", "origin", "main"]

    print(f"Pushing batch {index}/{total_batches}...")
    run_with_retries(push_command, repo_dir, f"Push batch {index}/{total_batches}")


def push_in_batches(repo_dir, remote_url):
    repo_files = iter_repo_files(repo_dir)
    batches = build_push_batches(repo_files)

    if not batches:
        raise RuntimeError("No files found to push.")

    total_batches = len(batches)

    if not is_git_repo(repo_dir):
        subprocess.run(["git", "init"], cwd=repo_dir, check=True)
        subprocess.run(["git", "branch", "-M", "main"], cwd=repo_dir, check=True)
    else:
        subprocess.run(["git", "branch", "-M", "main"], cwd=repo_dir, check=True)

    existing_origin = get_origin_url(repo_dir)
    if existing_origin is None:
        subprocess.run(["git", "remote", "add", "origin", remote_url], cwd=repo_dir, check=True)
    elif existing_origin != remote_url:
        raise RuntimeError(f"Existing origin remote does not match expected public remote: {existing_origin}")

    last_imported_batch, recorded_total_batches = get_last_imported_batch(repo_dir)
    start_index = 1
    if last_imported_batch is not None:
        if recorded_total_batches != total_batches:
            raise RuntimeError(
                f"Cannot resume import: existing repo expects {recorded_total_batches} batches but current run built {total_batches}."
            )

        if last_imported_batch >= total_batches:
            print("All batches are already committed locally. Ensuring they are pushed.\n")
            push_batch(repo_dir, total_batches, total_batches)
            return

        start_index = last_imported_batch + 1
        print(f"Resuming from batch {start_index}/{total_batches}.\n")

    for index in range(start_index, total_batches + 1):
        batch_paths = batches[index - 1]
        print(f"Staging batch {index}/{total_batches} ({len(batch_paths)} files)...")
        git_add_batch(repo_dir, batch_paths)
        subprocess.run(
            ["git", "commit", "-m", f"Import batch {index}/{total_batches}"],
            cwd=repo_dir,
            check=True,
        )

        push_batch(repo_dir, index, total_batches)


def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    is_resume = should_resume_import(script_dir, REMOTE_URL)

    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print()
    print("This script will perform DESTRUCTIVE actions:")
    print()
    if is_resume:
        print("- Resume the previously started public repo import")
        print("- Continue committing and pushing remaining batches to the PUBLIC repository")
    else:
        print("- Fetch and merge origin/leonadino into the current branch")
        print("- Reset tracked files to the last commit (git reset --hard HEAD)")
        print("- Delete all untracked and ignored files from the git working tree")
        print("- Delete all non-.meta files from licensed/third-party directories")
        print("- REMOVE the local .git repository entirely")
        print("- Initialize a brand new git repo and FORCE PUSH all local changes to the PUBLIC repository")
    print()
    if is_resume:
        print("A previous run was detected, so the destructive cleanup phase will be skipped")
        print("and the import will continue from the next unfinished batch.")
    else:
        print("This action is IRREVERSIBLE. All local git history will be lost")
        print("and the public repo will be overwritten.")
    print()
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print("-= WARNING WARNING WARNING WARNING WARNING WARNING =-")
    print()
    confirm = input('Type "I understand" to continue: ').strip()
    if confirm != "I understand":
        print("you didnt type it right")
        return

    if not is_resume:
        merge_leonadino()

        print("=== ! Nuking ! ===\n")

        reset_git_repo(script_dir)
        clean_git_repo(script_dir)

        for rel_path in DIRECTORIES:
            nuke(os.path.join(script_dir, rel_path))

        def remove_readonly(func, path, exc_info):
            os.chmod(path, stat.S_IWRITE)
            func(path)

        shutil.rmtree(os.path.join(script_dir, ".git"), onexc=remove_readonly)
    else:
        last_imported_batch, total_batches = get_last_imported_batch(script_dir)
        if last_imported_batch is not None and total_batches is not None:
            next_batch = min(last_imported_batch + 1, total_batches)
            print(f"=== Resuming from batch {next_batch}/{total_batches} ===\n")
        else:
            print("=== Resuming previous import ===\n")

    print("\n=== Removing local .git and force pushing to public repo ===\n")
    try:
        push_in_batches(script_dir, REMOTE_URL)
        print("\nForce push successful.")
    except subprocess.CalledProcessError as e:
        print(f"\nForce push failed: {e}")
        raise

    print("\nDone.")


if __name__ == "__main__":
    try:
        main()
    except Exception:
        print("\nAn error occurred:\n")
        traceback.print_exc()
        input("\nPress Enter to close...")
        raise
