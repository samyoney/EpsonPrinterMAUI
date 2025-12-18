const { spawn } = require("child_process");
const path = require("path");
const Table = require("cli-table3");
const chalk = require("chalk");

let testResults = [];

function createResultsTable() {
  const table = new Table({
    head: [
      chalk.blue("Test Name"),
      chalk.blue("Status"),
      chalk.blue("Duration"),
      chalk.blue("Message"),
    ],
    style: {
      head: [],
      border: [],
    },
  });

  testResults.forEach((test) => {
    table.push([
      chalk.white(test.name),
      test.passed ? chalk.green("✓ PASS") : chalk.red("✗ FAIL"),
      chalk.yellow(`${test.duration}ms`),
      test.message || "-",
    ]);
  });

  return table;
}

function runMauiApp() {
  console.log(chalk.blue("📱 Starting MAUI application..."));
  const mauiProcess = spawn(
    "dotnet",
    ["build", "-t:Run", "-f", "net8.0-android"],
    {
      cwd: path.join(__dirname, "EpsonPrinter"),
    }
  );

  mauiProcess.stdout.on("data", (data) => {
    console.log(chalk.cyan(`MAUI App: ${data}`));
  });

  mauiProcess.stderr.on("data", (data) => {
    console.error(chalk.red(`MAUI Error: ${data}`));
  });

  return new Promise((resolve) => {
    mauiProcess.on("close", resolve);
  });
}

function runTests() {
  console.log(chalk.blue("\n🧪 Running Tests..."));
  testResults = [];
  let testOutput = ""; 

  const testProcess = spawn("dotnet", [
    "test",
    path.join(__dirname, "ScannerApp.Tests", "ScannerApp.Tests.csproj"),
    "--logger",
    "console;verbosity=detailed",
    "--no-build",
    "--configuration",
    "Debug",
  ]);

  testProcess.stdout.on("data", (data) => {
    testOutput += data.toString();
  });

  testProcess.stderr.on("data", (data) => {
    console.error(chalk.red(`Test Error: ${data}`));
  });

  return new Promise((resolve) => {
    testProcess.on("close", (code) => {
  
      const lines = testOutput.split("\n");

      lines.forEach((line) => {
        const testPattern =
          /\s+(Passed|Failed)\s+([\w\.\d\s]+\w)\s+\[([\d<>\s]+)ms\]/;
        const match = line.match(testPattern);

        if (match) {
          const [_, status, testName, duration] = match;
          testResults.push({
            name: testName.trim(),
            passed: status === "Passed",
            duration: duration.trim(),
            message: status === "Passed" ? "-" : "Test failed",
          });
        }
      });

    
      testResults.sort((a, b) => a.name.localeCompare(b.name));

      console.log("\nTest Results:");
      console.log(createResultsTable().toString());

      const passedTests = testResults.filter((t) => t.passed).length;
      const totalTests = testResults.length;

      console.log(chalk.blue("\nSummary:"));
      console.log(chalk.green(`✓ Passed: ${passedTests}`));
      console.log(chalk.red(`✗ Failed: ${totalTests - passedTests}`));
      console.log(chalk.yellow(`Total: ${totalTests}`));

      resolve(code);
    });
  });
}

runMauiApp().then(() => {
  setTimeout(() => {
    runTests();
  }, 5000);
});
